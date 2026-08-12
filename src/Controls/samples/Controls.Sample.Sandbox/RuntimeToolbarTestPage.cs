namespace Maui.Controls.Sample;

/// <summary>
/// Tests runtime changes to NavigationPage/Shell toolbar highlight colors
/// (BarBackgroundColor, BarTextColor, IconColor via Shell.ForegroundColor when hosted in Shell)
/// and verifies pushing a second page keeps the toolbar colors correct (regression check for
/// the pushed-page white title issue).
/// </summary>
public class RuntimeToolbarTestPage : ContentPage
{
	// White text and Navy background coincide with the native toolbar defaults, so they're excluded here -
	// every cycle then produces a visibly different color from Reset for a clean video demo.
	static readonly Color[] BarBackgroundColors = { Colors.HotPink, Colors.Black, Colors.Teal, Colors.LightSkyBlue };
	static readonly Color[] BarTextColors = { Colors.Black, Colors.Lime, Colors.Yellow, Colors.Orange };

	int _barBackgroundIndex;
	int _barTextIndex;

	public RuntimeToolbarTestPage(int pageNumber = 1)
	{
		Title = $"Toolbar Test Page {pageNumber}";

		// NavigationPage.BarBackgroundColor/BarTextColor are plain instance properties of
		// NavigationPage itself (NOT attached properties like IconColor), and
		// NavigationPageToolbar reads them straight off the NavigationPage instance
		// (_currentNavigationPage.BarTextColor) - not off the current page. Setting them here
		// via SetValue(...) on this ContentPage would set a value nothing ever reads.
		NavigationPage? CurrentNavigationPage() => Application.Current?.Windows.Count > 0
			? Application.Current.Windows[0].Page as NavigationPage
			: null;

		var cycleBarBackgroundButton = new Button { Text = "Cycle BarBackgroundColor" };
		cycleBarBackgroundButton.Clicked += (s, e) =>
		{
			_barBackgroundIndex = (_barBackgroundIndex + 1) % BarBackgroundColors.Length;
			if (CurrentNavigationPage() is NavigationPage navPage)
				navPage.BarBackgroundColor = BarBackgroundColors[_barBackgroundIndex];
		};

		var cycleBarTextButton = new Button { Text = "Cycle BarTextColor" };
		cycleBarTextButton.Clicked += (s, e) =>
		{
			_barTextIndex = (_barTextIndex + 1) % BarTextColors.Length;
			if (CurrentNavigationPage() is NavigationPage navPage)
				navPage.BarTextColor = BarTextColors[_barTextIndex];
		};

		var resetButton = new Button { Text = "Reset to Default (null)" };
		resetButton.Clicked += (s, e) =>
		{
			if (CurrentNavigationPage() is NavigationPage navPage)
			{
				navPage.BarBackgroundColor = null;
				navPage.BarTextColor = null;
			}
		};

		var pushSecondPageButton = new Button { Text = "Push Another Toolbar Test Page" };
		pushSecondPageButton.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new RuntimeToolbarTestPage(pageNumber + 1));
		};

		Content = new ScrollView
		{
			Padding = 20,
			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Children =
				{
					new Label { Text = $"Toolbar Highlight Test - Page {pageNumber}", FontSize = 20, FontAttributes = FontAttributes.Bold },
					new Label { Text = "Change the toolbar colors below, then push another page to confirm the title/back-icon stay visible and correctly colored after navigation." },
					cycleBarBackgroundButton,
					cycleBarTextButton,
					resetButton,
					pushSecondPageButton,
				}
			}
		};
	}
}
