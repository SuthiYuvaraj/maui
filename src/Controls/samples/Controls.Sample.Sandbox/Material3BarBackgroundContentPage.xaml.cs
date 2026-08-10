namespace Maui.Controls.Sample;

public partial class Material3BarBackgroundContentPage : ContentPage
{
	public Material3BarBackgroundContentPage()
	{
		InitializeComponent();
	}

	void OnSetSolidBrushClicked(object? sender, EventArgs e)
	{
		if (Parent is not TabbedPage tabbedPage)
			return;

		tabbedPage.BarBackground = new SolidColorBrush(Colors.MidnightBlue);
	}

	void OnSetGradientBrushClicked(object? sender, EventArgs e)
	{
		if (Parent is not TabbedPage tabbedPage)
			return;

		tabbedPage.BarBackground = new LinearGradientBrush
		{
			GradientStops =
			{
				new GradientStop { Color = Colors.MidnightBlue, Offset = 0 },
				new GradientStop { Color = Colors.Purple, Offset = 1 },
			}
		};
	}

	void OnClearBrushClicked(object? sender, EventArgs e)
	{
		if (Parent is not TabbedPage tabbedPage)
			return;

		tabbedPage.ClearValue(TabbedPage.BarBackgroundProperty);
	}

	void OnBackClicked(object? sender, EventArgs e) =>
		App.ShowSample(new Material3TestMenuPage());
}
