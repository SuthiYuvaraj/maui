<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

# Toolbar `BarTextColor` Reset Bug (Android) — Root Cause & Fix

## Summary

When `Toolbar.BarTextColor` (and the paired back-button icon color) is set at
runtime and then cleared back to `null` (e.g. via `ClearValue` on a
`NavigationPage`/`TabbedPage`, or a Shell page's appearance resetting),
the **toolbar title text does not revert to its native/theme default color**
on Android. The back-button (`DrawerArrowDrawable`) icon has the same class of
bug, but it was already fixed for the Shell code path — the title text bug is
the same root cause, in a different, non-Shell-only location.

This was discovered while manually regression-testing the `shell_pocnull`
branch (which replaced hardcoded Material2-era default colors with
captured-native-value/restore-on-null logic) using a Sandbox test harness
(`RuntimeToolbarTestPage`, `RuntimeTabbedPageTest`). It reproduces **identically
on `main`**, so it is a **pre-existing framework bug**, not a regression
introduced by that branch.

## Reproduction

1. Host a `ContentPage` in a plain `NavigationPage` (no Shell).
2. Set `NavigationPage.BarTextColor` to a custom color (e.g. `Colors.Lime`).
   - Title text and back-arrow icon both turn lime. ✅ Correct.
3. Set `NavigationPage.BarTextColor = null` (or `ClearValue(BarTextColorProperty)`).
   - **Back-arrow icon correctly reverts to its native default color.** ✅
   - **Title text stays lime — it never reverts.** ❌ Bug.

Confirmed via on-device A/B testing (swap in `main`'s
`ToolbarExtensions.cs`, rebuild, install, retest): the exact same broken
behavior occurs on `main`. In fact, on `main` **both** the icon and the title
fail to reset (the icon-reset fix landed on this branch already, described
below).

## Root Cause

The color-reset logic lives in
`src/Controls/src/Core/Platform/Android/Extensions/ToolbarExtensions.cs`,
`UpdateBarTextColor(this AToolbar nativeToolbar, Toolbar toolbar)`:

```csharp
public static void UpdateBarTextColor(this AToolbar nativeToolbar, Toolbar toolbar)
{
    var textColor = toolbar.BarTextColor;

    // Because we use the same toolbar across multiple navigation pages (think tabbed page with nested NavigationPage)
    // We need to reset the toolbar text color to the default color when it's unset
    if (_defaultTitleTextColor == null)
    {
        var context = nativeToolbar.Context?.GetThemedContext();
        _defaultTitleTextColor = PlatformInterop.GetColorStateListForToolbarStyleableAttribute(context,
            Resource.Attribute.toolbarStyle, Resource.Styleable.Toolbar_titleTextColor);
    }

    if (textColor != null)
    {
        nativeToolbar.SetTitleTextColor(textColor.ToPlatform().ToArgb());
    }
    else if (_defaultTitleTextColor != null)
    {
        nativeToolbar.SetTitleTextColor(_defaultTitleTextColor);
    }
    ...
}
```

- `_defaultTitleTextColor` is a **static field**, captured **once**, the first
  time any toolbar's text color is updated, and reused for the lifetime of the
  process.
- The capture reads the AppCompat `Toolbar_titleTextColor` **styleable
  attribute** off the theme's `toolbarStyle`.
- **Under Material3, this attribute is typically not set directly** — M3
  themes drive the title color through `titleTextAppearance` instead of the
  plain `titleTextColor` styleable. So
  `PlatformInterop.GetColorStateListForToolbarStyleableAttribute(...)` returns
  `null` on first call.
- Because the field is only ever set once (`if (_defaultTitleTextColor ==
  null)`), and the lookup returned `null`, **`_defaultTitleTextColor` stays
  `null` forever**. The `else if (_defaultTitleTextColor != null)` restore
  branch can then never fire — there is no path back to the native default
  once `BarTextColor` has been set at least once, for the remainder of the
  app's process lifetime.

### Why the icon doesn't have this problem (on this branch)

The same method has a parallel block for the back-button icon:

```csharp
if (nativeToolbar.NavigationIcon is DrawerArrowDrawable icon)
{
    if (textColor != null)
    {
        _defaultNavigationIconColor = icon.Color;      // <-- was a plain assignment
        icon.Color = textColor.ToPlatform().ToArgb();
    }
    else if (_defaultNavigationIconColor != null)
    {
        icon.Color = _defaultNavigationIconColor.Value;
    }
}
```

This branch was already patched on this branch to capture **once** instead of
on every call:

```csharp
_defaultNavigationIconColor ??= icon.Color;
```

That fix works because it captures the icon's **live, already-rendered
native color** directly from the `DrawerArrowDrawable` instance — it doesn't
depend on a theme-styleable lookup that can silently return null under M3.
The title-text path has no equivalent live-value capture; it only has the
styleable lookup, which is the actual root cause.

### Why Shell doesn't hit this

`src/Controls/src/Core/Compatibility/Handlers/Shell/Android/ShellToolbarAppearanceTracker.cs`
already solves the identical problem for **Shell** pages, by capturing the
real M3 default from the theme's `colorOnSurface` attribute instead of the
broken AppCompat styleable:

```csharp
// Shell.Toolbar.BarTextColor is a cross-platform Color, so unlike the TabLayout/BottomNavigationView
// trackers (which can poke native ColorStateLists directly), a null title color here still has to
// flow through ToolbarExtensions.UpdateBarTextColor. That method's own "no color set" fallback
// queries an AppCompat Toolbar styleable that isn't Material3-aware and resolves to the wrong
// (near-invisible) color under M3. Capturing the real M3 theme color once and using it as our
// default keeps BarTextColor from ever being null, so that broken shared fallback path is never hit.
internal void CaptureNativeColors(AToolbar toolbar)
{
    if (_originalAppearanceCaptured)
        return;

    var context = toolbar?.Context;
    if (context is not null)
        _originalNativeTitleColor = Color.FromInt(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface));
    ...
    _originalAppearanceCaptured = true;
}
```

`ShellToolbarAppearanceTracker.SetColors()`/`RestoreNativeColors()` then
**always** pass a non-null `BarTextColor` down into
`Toolbar.BarTextColor` (falling back to `_originalNativeTitleColor` instead of
`null`), which means the broken `_defaultTitleTextColor` fallback path inside
`ToolbarExtensions.UpdateBarTextColor` is **never actually exercised** for
Shell — Shell routes around the bug rather than fixing it at its source.

**This is exactly why the bug reappears for `RuntimeToolbarTestPage`:** that
page uses a **plain `NavigationPage`** (no Shell at all). There is no
`ShellToolbarAppearanceTracker` instance in that code path — Shell's
workaround simply doesn't apply. The only remaining default/restore logic is
the shared, still-broken one inside `ToolbarExtensions.UpdateBarTextColor`
itself, so the bug is fully exposed there.

| Layer | Capture mechanism | Scenario covered |
| --- | --- | --- |
| `ShellToolbarAppearanceTracker._originalNativeTitleColor` | `colorOnSurface` theme attr (already correct) | Shell pages only |
| `ToolbarExtensions._defaultTitleTextColor` | AppCompat `Toolbar_titleTextColor` styleable (returns `null` under M3) | **Every other toolbar-hosting page**: plain `NavigationPage`, `TabbedPage`'s nested `NavigationPage`, etc. |

Two independent "capture native default once, restore on null" mechanisms
exist at two different layers of the stack. Shell's tracker fixed its own
layer; the shared, lower-level `ToolbarExtensions` layer was never fixed, so
any non-Shell toolbar still hits the original bug.

## Fix

Mirror the same fallback Shell already uses — when
`RuntimeFeature.IsMaterial3Enabled` is true, capture `colorOnSurface` from the
theme instead of relying on the AppCompat styleable lookup that returns `null`
under M3:

```csharp
if (_defaultTitleTextColor == null)
{
    var context = nativeToolbar.Context?.GetThemedContext();

    if (RuntimeFeature.IsMaterial3Enabled)
    {
        // Under Material3 the AppCompat Toolbar_titleTextColor styleable attribute is typically not
        // set directly (M3 themes drive the title color through titleTextAppearance instead), so the
        // styleable lookup below returns null here. Since this default is only ever captured once
        // (it's a static field reused across toolbars/pages), a null result here would permanently
        // break the "reset to default" path for every toolbar in the app. Fall back to the theme's
        // colorOnSurface, which is what M3 toolbars actually render for their title by default -
        // the same value ShellToolbarAppearanceTracker.CaptureNativeColors uses for the same reason.
        if (context is not null)
            _defaultTitleTextColor = ColorStateList.ValueOf(new AGraphics.Color(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface)));
    }
    else
    {
        _defaultTitleTextColor = PlatformInterop.GetColorStateListForToolbarStyleableAttribute(context,
            Resource.Attribute.toolbarStyle, Resource.Styleable.Toolbar_titleTextColor);
    }
}
```

This keeps the non-M3 (Material2) path completely unchanged, and only adds
the missing M3 fallback that Shell already relies on — using the exact same
`colorOnSurface` theme attribute and `GetThemeAttrColor` helper.

## Status

- **Root cause confirmed** and **fix drafted** (see diff above).
- Fix was applied once and *appeared* to have no effect on-device, but that
  was actually caused by a **stale/incrementally-packaged APK** (the Android
  build's incremental packaging did not repick the freshly rebuilt
  `Microsoft.Maui.Controls.dll` on first install) — not a flaw in the fix
  itself. A full rebuild (`-t:Rebuild`) was required to get a fresh APK, but a
  clean re-test was then blocked by an unrelated environment issue (a stale
  Fast-Deployment `.__override__` directory on the emulator causing
  `monodroid: No assemblies found ... Exiting`, and a separate interactive
  permission-prompt failure in the tool sandbox).
- **The fix has not yet been re-verified on-device end-to-end.** Next steps:
  1. Re-apply the diff above to
     `src/Controls/src/Core/Platform/Android/Extensions/ToolbarExtensions.cs`
     (it was reverted by an external edit during this investigation).
  2. Force a full `-t:Rebuild` of `Controls.Sample.Sandbox` for
     `net10.0-android`.
  3. `adb uninstall` the sandbox package first (to avoid any Fast Deployment
     `.__override__` cache staleness), then fresh `adb install`.
  4. Re-run the `RuntimeToolbarTestPage` repro: cycle `BarTextColor`, then
     Reset, and confirm the title text (not just the icon) reverts to its
     native default color.
  5. Also re-verify `RuntimeTabbedPageTest`'s BarTextColor cycle/reset, and
     the original Shell scenarios, to confirm no regression from this change.
