namespace Maui.Controls.Sample;

/// <summary>
/// Shell scenario exercising the Flyout appearance (Shell.FlyoutBackgroundColor/FlyoutBackground,
/// Shell.BackgroundColor/ForegroundColor/TitleColor for the flyout header nav-bar) to verify
/// runtime color changes and reset-to-default apply correctly when a Flyout is present
/// (FlyoutBehavior=Flyout with multiple FlyoutItems).
/// </summary>
public class RuntimeShellFlyoutTestShell : Shell
{
	static readonly Color[] FlyoutBackgroundColors = { Colors.Navy, Colors.DarkGreen, Colors.Coral, Colors.SlateGray };
	static readonly Color[] ForegroundColors = { Colors.Green, Colors.Red, Colors.Blue, Colors.Purple };
	static readonly Color[] BackgroundColors = { Colors.Teal, Colors.Beige, Colors.Coral, Colors.DarkSeaGreen };
	static readonly Color[] TitleColors = { Colors.Yellow, Colors.Orange, Colors.Cyan, Colors.HotPink };

	static readonly Style HighlightStyle = new(typeof(Shell))
	{
		Setters =
		{
			new Setter { Property = FlyoutBackgroundColorProperty, Value = Colors.DarkSlateGray },
			new Setter { Property = ForegroundColorProperty, Value = Colors.Yellow },
			new Setter { Property = BackgroundColorProperty, Value = Colors.DarkSlateBlue },
			new Setter { Property = TitleColorProperty, Value = Colors.Gold },
		}
	};

	int _flyoutBackgroundIndex;
	int _foregroundIndex;
	int _backgroundIndex;
	int _titleIndex;
	bool _styleApplied;

	public RuntimeShellFlyoutTestShell()
	{
		Title = "Shell Flyout Highlight Test";
		FlyoutBehavior = FlyoutBehavior.Flyout;

		for (int i = 1; i <= 3; i++)
		{
			var item = new FlyoutItem { Title = $"Flyout Item {i}", Icon = "dotnet_bot.png" };
			item.Items.Add(new ShellContent
			{
				Title = $"Flyout Item {i}",
				ContentTemplate = new DataTemplate(CreateContentPage),
				Route = $"RuntimeShellFlyoutItem{i}",
			});
			Items.Add(item);
		}
	}

	ContentPage CreateContentPage()
	{
		var cycleFlyoutBackgroundButton = new Button { Text = "Cycle FlyoutBackgroundColor" };
		cycleFlyoutBackgroundButton.Clicked += (s, e) =>
		{
			_flyoutBackgroundIndex = (_flyoutBackgroundIndex + 1) % FlyoutBackgroundColors.Length;
			FlyoutBackgroundColor = FlyoutBackgroundColors[_flyoutBackgroundIndex];
		};

		var cycleForegroundButton = new Button { Text = "Cycle ForegroundColor (nav-bar icons)" };
		cycleForegroundButton.Clicked += (s, e) =>
		{
			_foregroundIndex = (_foregroundIndex + 1) % ForegroundColors.Length;
			SetForegroundColor(this, ForegroundColors[_foregroundIndex]);
		};

		var cycleBackgroundButton = new Button { Text = "Cycle BackgroundColor (nav-bar)" };
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
			ClearValue(FlyoutBackgroundColorProperty);
			ClearValue(ForegroundColorProperty);
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
			Content = new ScrollView
			{
				Padding = 20,
				Content = new VerticalStackLayout
				{
					Spacing = 12,
					Children =
					{
						new Label { Text = "Shell Flyout Highlight Test", FontSize = 20, FontAttributes = FontAttributes.Bold },
						new Label { Text = "Open the Flyout (hamburger icon / swipe from left) to see the flyout background color. Change colors below, then reopen the Flyout and check the nav-bar to confirm updates apply at runtime." },
						cycleFlyoutBackgroundButton,
						cycleForegroundButton,
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
