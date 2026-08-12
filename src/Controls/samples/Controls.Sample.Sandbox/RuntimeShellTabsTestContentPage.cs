namespace Maui.Controls.Sample;

/// <summary>
/// Shared content page for each tab in <see cref="RuntimeShellTabsTestShell"/>.
/// Provides buttons to change Shell TabBar highlight colors at runtime
/// (Shell.TabBarForegroundColor, Shell.TabBarUnselectedColor, Shell.TabBarBackgroundColor,
/// Shell.TabBarTitleColor) so the resulting bottom/top tab UI can be verified for regressions.
/// </summary>
public class RuntimeShellTabsTestContentPage : ContentPage
{
	static readonly Color[] ForegroundColors = { Colors.Green, Colors.Red, Colors.Blue, Colors.Purple };
	static readonly Color[] UnselectedColors = { Colors.Gray, Colors.LightGray, Colors.Silver, Colors.DarkGray };
	static readonly Color[] BackgroundColors = { Colors.Navy, Colors.Beige, Colors.Coral, Colors.DarkSeaGreen };
	static readonly Color[] TitleColors = { Colors.Yellow, Colors.Orange, Colors.Cyan, Colors.HotPink };

	static readonly Style HighlightStyle = new(typeof(Page))
	{
		Setters =
		{
			new Setter { Property = Shell.TabBarForegroundColorProperty, Value = Colors.Magenta },
			new Setter { Property = Shell.TabBarUnselectedColorProperty, Value = Colors.LightPink },
			new Setter { Property = Shell.TabBarBackgroundColorProperty, Value = Colors.DarkSlateBlue },
			new Setter { Property = Shell.TabBarTitleColorProperty, Value = Colors.Gold },
		}
	};

	int _foregroundIndex;
	int _unselectedIndex;
	int _backgroundIndex;
	int _titleIndex;
	bool _styleApplied;

	public RuntimeShellTabsTestContentPage()
	{
		// Shell.TabBarXxxColor are Shell-attached properties whose change-propagation walks up
		// from the element they were set on to find the owning Shell (OnShellAppearanceValueChanged
		// in Shell.cs). Setting them on `this` (the individual ContentPage instance created per
		// tab via ContentTemplate) does NOT reliably reach the BottomNavigationView/TabLayout
		// appearance trackers - those trackers observe a pivot registered higher up the tree, and
		// the walk from a leaf ContentPage doesn't consistently intersect it (same root-cause
		// class as the NavigationPage.BarTextColor "must set on the actual NavigationPage instance,
		// not the displayed ContentPage" issue in RuntimeToolbarTestPage). Setting directly on
		// Shell.Current (matching the working RuntimeShellFlyoutTestShell pattern, which sets on
		// the Shell instance itself) reliably notifies every tab.
		var cycleForegroundButton = new Button { Text = "Cycle TabBarForegroundColor (selected)" };
		cycleForegroundButton.Clicked += (s, e) =>
		{
			_foregroundIndex = (_foregroundIndex + 1) % ForegroundColors.Length;
			if (Shell.Current is Shell shell)
				Shell.SetTabBarForegroundColor(shell, ForegroundColors[_foregroundIndex]);
		};

		var cycleUnselectedButton = new Button { Text = "Cycle TabBarUnselectedColor" };
		cycleUnselectedButton.Clicked += (s, e) =>
		{
			_unselectedIndex = (_unselectedIndex + 1) % UnselectedColors.Length;
			if (Shell.Current is Shell shell)
				Shell.SetTabBarUnselectedColor(shell, UnselectedColors[_unselectedIndex]);
		};

		var cycleBackgroundButton = new Button { Text = "Cycle TabBarBackgroundColor" };
		cycleBackgroundButton.Clicked += (s, e) =>
		{
			_backgroundIndex = (_backgroundIndex + 1) % BackgroundColors.Length;
			if (Shell.Current is Shell shell)
				Shell.SetTabBarBackgroundColor(shell, BackgroundColors[_backgroundIndex]);
		};

		var cycleTitleButton = new Button { Text = "Cycle TabBarTitleColor" };
		cycleTitleButton.Clicked += (s, e) =>
		{
			_titleIndex = (_titleIndex + 1) % TitleColors.Length;
			if (Shell.Current is Shell shell)
				Shell.SetTabBarTitleColor(shell, TitleColors[_titleIndex]);
		};

		var styleButton = new Button { Text = "Apply Style (Setters)" };
		styleButton.Clicked += (s, e) =>
		{
			_styleApplied = !_styleApplied;
			if (Shell.Current is Shell shell)
				shell.Style = _styleApplied ? HighlightStyle : null;
			((Button)s!).Text = _styleApplied ? "Remove Style (Setters)" : "Apply Style (Setters)";
		};

		var resetButton = new Button { Text = "Reset to Default (null)" };
		resetButton.Clicked += (s, e) =>
		{
			_styleApplied = false;
			if (Shell.Current is Shell shell)
			{
				shell.Style = null;
				shell.ClearValue(Shell.TabBarForegroundColorProperty);
				shell.ClearValue(Shell.TabBarUnselectedColorProperty);
				shell.ClearValue(Shell.TabBarBackgroundColorProperty);
				shell.ClearValue(Shell.TabBarTitleColorProperty);
			}
		};

		var backToLauncherButton = new Button { Text = "Back to Launcher" };
		backToLauncherButton.Clicked += (s, e) =>
		{
			if (Application.Current?.Windows.Count > 0)
				Application.Current.Windows[0].Page = new NavigationPage(new RuntimeAppearanceTestLauncherPage());
		};

		Content = new ScrollView
		{
			Padding = 20,
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Children =
				{
					new Label { Text = "Shell Tabs Highlight Test", FontSize = 20, FontAttributes = FontAttributes.Bold },
					new Label { Text = "Change the highlight colors below, then switch tabs to confirm the selected/unselected tab-bar colors update at runtime." },
					cycleForegroundButton,
					cycleUnselectedButton,
					cycleBackgroundButton,
					cycleTitleButton,
					styleButton,
					resetButton,
					backToLauncherButton,
				}
			}
		};
	}
}
