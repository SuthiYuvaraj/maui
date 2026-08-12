#nullable disable
using Android.Content.Res;
using Android.Graphics.Drawables;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;
using R = Android.Resource;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTabLayoutAppearanceTracker : IShellTabLayoutAppearanceTracker
	{
		bool _disposed;
		bool _originalAppearanceCaptured;
		ColorStateList _originalTextColors;
		Drawable _originalBackground;
		Drawable _originalIndicatorDrawable;
		int? _originalIndicatorColor;
		IShellContext _shellContext;

		public ShellTabLayoutAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void ResetAppearance(TabLayout tabLayout)
		{
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				RestoreNativeColors(tabLayout);
			}
			else
			{
				SetColors(tabLayout, ShellRenderer.DefaultForegroundColor,
					ShellRenderer.DefaultBackgroundColor,
					ShellRenderer.DefaultTitleColor,
					ShellRenderer.DefaultUnselectedColor);
			}
		}

		public virtual void SetAppearance(TabLayout tabLayout, ShellAppearance appearance)
		{
			var foreground = appearance.ForegroundColor;
			var background = appearance.BackgroundColor;
			var titleColor = appearance.TitleColor;
			var unselectedColor = appearance.UnselectedColor;

			SetColors(tabLayout, foreground, background, titleColor, unselectedColor);
		}

		protected virtual void SetColors(TabLayout tabLayout, Color foreground, Color background, Color title, Color unselected)
		{
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				// Derive the selected-state color from the captured native ColorStateList on demand,
				// using its own DefaultColor as the GetColorForState fallback (matches the pattern used
				// by ShellBottomNavViewAppearanceTracker.MakeColorStateList). CaptureNativeColors always
				// runs before SetColors on Material3, so _originalTextColors is guaranteed non-null here.
				var materialTitleArgb = title?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;
				var materialUnselectedArgb = unselected?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;

				tabLayout.SetTabTextColors(materialUnselectedArgb, materialTitleArgb);

				if (background is null)
				{
					tabLayout.SetBackground(_originalBackground);
				}
				else
				{
					tabLayout.SetBackground(new ColorDrawable(background.ToPlatform()));
				}

				if (foreground is not null)
				{
					tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform());
				}
				else
				{
					// TabLayout.SetSelectedTabIndicator(Drawable) unconditionally re-tints whatever
					// Drawable it's given using TabLayout's own persistent tabSelectedIndicatorColor
					// field (see DrawableUtils.setTint call inside AndroidX's implementation) - it does
					// NOT clear/reset that field. So restoring a pristine Drawable clone alone isn't
					// enough: the stale color field (left over from the last SetSelectedTabIndicatorColor
					// call above) immediately re-tints it back to the last custom color. We have to
					// explicitly reset the color field too, using the native default color captured by
					// CaptureNativeColors.
					tabLayout.SetSelectedTabIndicator(_originalIndicatorDrawable);
					if (_originalIndicatorColor is int originalColor)
						tabLayout.SetSelectedTabIndicatorColor(new AColor(originalColor));
				}
			}
			else
			{
				var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
				var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();
				tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
				tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
				tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));
			}
		}

		internal void CaptureNativeColors(TabLayout tabLayout)
		{
			if (_originalAppearanceCaptured)
				return;

			_originalTextColors = tabLayout.TabTextColors;
			_originalBackground = tabLayout.Background;
			var liveIndicator = tabLayout.TabSelectedIndicator;
			_originalIndicatorDrawable = liveIndicator?.GetConstantState()?.NewDrawable() ?? liveIndicator;

			// TabLayout has no public getter for its private tabSelectedIndicatorColor field, but that
			// field is what SetSelectedTabIndicator(Drawable) always re-applies as a tint (see comment
			// in SetColors' else branch above), so we must capture it via reflection here - the same
			// technique already used for BottomNavigationMenuView's private mShiftingMode field in
			// BottomNavigationViewUtils - in order to be able to restore the true native default later.
			try
			{
				using var field = tabLayout.Class.GetDeclaredField("tabSelectedIndicatorColor");
				field.Accessible = true;
				_originalIndicatorColor = field.GetInt(tabLayout);
			}
			catch
			{
				_originalIndicatorColor = null;
			}

			_originalAppearanceCaptured = true;
		}

		void RestoreNativeColors(TabLayout tabLayout)
		{
			if (!_originalAppearanceCaptured)
				return;

			tabLayout.TabTextColors = _originalTextColors;
			tabLayout.SetBackground(_originalBackground);
			tabLayout.SetSelectedTabIndicator(_originalIndicatorDrawable);

			// See the matching comment in SetColors' else branch - SetSelectedTabIndicator always
			// re-tints using the stale tabSelectedIndicatorColor field, so the color must be reset
			// explicitly too, or the indicator will keep showing the last custom color.
			if (_originalIndicatorColor is int originalColor)
				tabLayout.SetSelectedTabIndicatorColor(new AColor(originalColor));
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			_originalBackground = null;
			_originalTextColors = null;
			_originalIndicatorDrawable = null;
			_originalIndicatorColor = null;
			_shellContext = null;
		}

		#endregion IDisposable
	}
}