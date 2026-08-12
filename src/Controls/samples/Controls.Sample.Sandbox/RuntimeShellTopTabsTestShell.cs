namespace Maui.Controls.Sample;

/// <summary>
/// Shell scenario that renders a **top** tab bar (Android <c>TabLayout</c> via
/// <c>ShellSectionRenderer</c>) by putting multiple <see cref="ShellContent"/> items inside a
/// single <see cref="Tab"/>. Contrast with <see cref="RuntimeShellTabsTestShell"/>, which puts
/// multiple <see cref="Tab"/> items inside the <see cref="TabBar"/> and renders a **bottom** tab
/// bar (Android <c>BottomNavigationView</c> via <c>ShellItemRenderer</c>).
///
/// IMPORTANT: unlike the bottom tab bar (which reads the TabBar-specific
/// Shell.TabBarForegroundColor/TabBarBackgroundColor/TabBarTitleColor/TabBarUnselectedColor,
/// falling back to the plain Shell.ForegroundColor/etc. when unset - see
/// IShellAppearanceElement.EffectiveTabBarXxxColor), the TOP tab bar's
/// ShellTabLayoutAppearanceTracker.SetAppearance only ever reads the PLAIN
/// Shell.ForegroundColor/BackgroundColor/TitleColor/UnselectedColor - it does not consult the
/// TabBar-prefixed properties or their Effective* fallback at all. This is existing framework
/// behavior (confirmed unchanged vs. main), not a bug introduced here - but it means this test
/// page must cycle the plain Shell color properties, not Shell.TabBarXxxColor, to actually see
/// the TOP tab bar respond.
/// </summary>
public class RuntimeShellTopTabsTestShell : Shell
{
	static readonly Color[] ForegroundColors = { Colors.Green, Colors.Red, Colors.Blue, Colors.Purple };
	static readonly Color[] UnselectedColors = { Colors.Gray, Colors.LightGray, Colors.Silver, Colors.DarkGray };
	static readonly Color[] BackgroundColors = { Colors.Navy, Colors.Beige, Colors.Coral, Colors.DarkSeaGreen };
	static readonly Color[] TitleColors = { Colors.Yellow, Colors.Orange, Colors.Cyan, Colors.HotPink };

	static readonly Style HighlightStyle = new(typeof(Shell))
	{
		Setters =
		{
			new Setter { Property = ForegroundColorProperty, Value = Colors.Magenta },
			new Setter { Property = UnselectedColorProperty, Value = Colors.LightPink },
			new Setter { Property = BackgroundColorProperty, Value = Colors.DarkSlateBlue },
			new Setter { Property = TitleColorProperty, Value = Colors.Gold },
		}
	};

	int _foregroundIndex;
	int _unselectedIndex;
	int _backgroundIndex;
	int _titleIndex;
	bool _styleApplied;

	public RuntimeShellTopTabsTestShell()
	{
		Title = "Shell Top Tabbar Highlight Test";

		var tabBar = new TabBar();
		var topTab = new Tab { Title = "Top Tabs" };

		for (int i = 1; i <= 3; i++)
		{
			topTab.Items.Add(new ShellContent
			{
				Title = $"Top {i}",
				Icon = "dotnet_bot.png",
				ContentTemplate = new DataTemplate(() => CreateContentPage(i)),
				Route = $"RuntimeShellTopTab{i}",
			});
		}

		tabBar.Items.Add(topTab);
		Items.Add(tabBar);
	}

	ContentPage CreateContentPage(int number)
	{
		var cycleForegroundButton = new Button { Text = "Cycle ForegroundColor (selected indicator)" };
		cycleForegroundButton.Clicked += (s, e) =>
		{
			_foregroundIndex = (_foregroundIndex + 1) % ForegroundColors.Length;
			SetForegroundColor(this, ForegroundColors[_foregroundIndex]);
		};

		var cycleUnselectedButton = new Button { Text = "Cycle UnselectedColor" };
		cycleUnselectedButton.Clicked += (s, e) =>
		{
			_unselectedIndex = (_unselectedIndex + 1) % UnselectedColors.Length;
			SetUnselectedColor(this, UnselectedColors[_unselectedIndex]);
		};

		var cycleBackgroundButton = new Button { Text = "Cycle BackgroundColor (tab strip)" };
		cycleBackgroundButton.Clicked += (s, e) =>
		{
			_backgroundIndex = (_backgroundIndex + 1) % BackgroundColors.Length;
			SetBackgroundColor(this, BackgroundColors[_backgroundIndex]);
		};

		var cycleTitleButton = new Button { Text = "Cycle TitleColor" };
		cycleTitleButton.Clicked += (s, e) =>
		{
			_titleIndex = (_titleIndex + 1) % TitleColors.Length;
			SetTitleColor(this, TitleColors[_titleIndex]);
		};

		var styleButton = new Button { Text = "Apply Style (Setters)" };
		styleButton.Clicked += (s, e) =>
		{
			_styleApplied = !_styleApplied;
			this.Style = _styleApplied ? HighlightStyle : null;
			((Button)s!).Text = _styleApplied ? "Remove Style (Setters)" : "Apply Style (Setters)";
		};

		var resetButton = new Button { Text = "Reset to Default (null)" };
		resetButton.Clicked += (s, e) =>
		{
			this.Style = null;
			_styleApplied = false;
			ClearValue(ForegroundColorProperty);
			ClearValue(UnselectedColorProperty);
			ClearValue(BackgroundColorProperty);
			ClearValue(TitleColorProperty);
		};

		var backToLauncherButton = new Button { Text = "Back to Launcher" };
		backToLauncherButton.Clicked += (s, e) =>
		{
			if (Application.Current?.Windows.Count > 0)
				Application.Current.Windows[0].Page = new NavigationPage(new RuntimeAppearanceTestLauncherPage());
		};

		return new ContentPage
		{
			Title = $"Top {number}",
			Content = new ScrollView
			{
				Padding = 20,
				Content = new VerticalStackLayout
				{
					Spacing = 12,
					Children =
					{
						new Label { Text = "Shell Top Tabbar Highlight Test", FontSize = 20, FontAttributes = FontAttributes.Bold },
						new Label { Text = "This section has multiple ShellContent items, so Android renders it as a TOP tab bar (TabLayout). Unlike the bottom tab bar, the top TabLayout only reads the plain Shell.ForegroundColor/BackgroundColor/TitleColor/UnselectedColor (NOT the TabBarXxxColor variants). Change colors below and swipe between top tabs to confirm updates apply at runtime." },
						cycleForegroundButton,
						cycleUnselectedButton,
						cycleBackgroundButton,
						cycleTitleButton,
						styleButton,
						resetButton,
						backToLauncherButton,
					}
				}
			}
		};
	}
}
