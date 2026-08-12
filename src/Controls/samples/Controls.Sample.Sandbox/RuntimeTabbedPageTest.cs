namespace Maui.Controls.Sample;

/// <summary>
/// Tests runtime changes to TabbedPage highlight colors (SelectedTabColor, UnselectedTabColor,
/// BarBackgroundColor, BarTextColor) to verify the platform tab-bar updates correctly after
/// the appearance colors change (not just at initial load).
/// </summary>
public class RuntimeTabbedPageTest : TabbedPage
{
	static readonly Color[] SelectedColors = { Colors.Red, Colors.Blue, Colors.Green, Colors.Purple };
	static readonly Color[] UnselectedColors = { Colors.Gray, Colors.LightGray, Colors.Silver, Colors.DarkGray };
	// White text and Navy background coincide with the native toolbar defaults, so they're excluded here -
	// every cycle then produces a visibly different color from Reset for a clean video demo.
	static readonly Color[] BarBackgroundColors = { Colors.HotPink, Colors.Black, Colors.Teal, Colors.Beige };
	static readonly Color[] BarTextColors = { Colors.Black, Colors.Lime, Colors.Yellow, Colors.Orange };

	static readonly Style HighlightStyle = new(typeof(TabbedPage))
	{
		Setters =
		{
			new Setter { Property = SelectedTabColorProperty, Value = Colors.Magenta },
			new Setter { Property = UnselectedTabColorProperty, Value = Colors.LightPink },
			new Setter { Property = BarBackgroundColorProperty, Value = Colors.DarkSlateBlue },
			new Setter { Property = BarTextColorProperty, Value = Colors.White },
		}
	};

	int _selectedIndex;
	int _unselectedIndex;
	int _barBackgroundIndex;
	int _barTextIndex;
	bool _styleApplied;

	public RuntimeTabbedPageTest()
	{
		Title = "TabbedPage Highlight Test";
		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Top);

		for (int i = 1; i <= 3; i++)
			Children.Add(CreateTab(i));
	}

	ContentPage CreateTab(int number)
	{
		var page = new ContentPage { Title = $"Tab {number}", IconImageSource = "dotnet_bot.png" };

		var cycleSelectedButton = new Button { Text = "Cycle SelectedTabColor" };
		cycleSelectedButton.Clicked += (s, e) =>
		{
			_selectedIndex = (_selectedIndex + 1) % SelectedColors.Length;
			SelectedTabColor = SelectedColors[_selectedIndex];
		};

		var cycleUnselectedButton = new Button { Text = "Cycle UnselectedTabColor" };
		cycleUnselectedButton.Clicked += (s, e) =>
		{
			_unselectedIndex = (_unselectedIndex + 1) % UnselectedColors.Length;
			UnselectedTabColor = UnselectedColors[_unselectedIndex];
		};

		var cycleBarBackgroundButton = new Button { Text = "Cycle BarBackgroundColor" };
		cycleBarBackgroundButton.Clicked += (s, e) =>
		{
			_barBackgroundIndex = (_barBackgroundIndex + 1) % BarBackgroundColors.Length;
			BarBackgroundColor = BarBackgroundColors[_barBackgroundIndex];
		};

		var cycleBarTextButton = new Button { Text = "Cycle BarTextColor" };
		cycleBarTextButton.Clicked += (s, e) =>
		{
			_barTextIndex = (_barTextIndex + 1) % BarTextColors.Length;
			BarTextColor = BarTextColors[_barTextIndex];
		};

		var styleButton = new Button { Text = "Apply Style (Setters)" };
		styleButton.Clicked += (s, e) =>
		{
			_styleApplied = !_styleApplied;
			Style = _styleApplied ? HighlightStyle : null;
			((Button)s!).Text = _styleApplied ? "Remove Style (Setters)" : "Apply Style (Setters)";
		};

		var resetButton = new Button { Text = "Reset to Default (null)" };
		resetButton.Clicked += (s, e) =>
		{
			Style = null;
			_styleApplied = false;
			ClearValue(SelectedTabColorProperty);
			ClearValue(UnselectedTabColorProperty);
			ClearValue(BarBackgroundColorProperty);
			ClearValue(BarTextColorProperty);
		};

		page.Content = new ScrollView
		{
			Padding = 20,
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Children =
				{
					new Label { Text = $"Tab {number}", FontSize = 20, FontAttributes = FontAttributes.Bold },
					new Label { Text = "Change the highlight colors below, then switch tabs to confirm the selected/unselected tab indicators update at runtime." },
					cycleSelectedButton,
					cycleUnselectedButton,
					cycleBarBackgroundButton,
					cycleBarTextButton,
					styleButton,
					resetButton,
				}
			}
		};

		return page;
	}
}
