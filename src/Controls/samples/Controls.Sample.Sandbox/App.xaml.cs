namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// To test runtime highlight-color changes for TabbedPage/Shell tabs/Toolbar,
		// launch the dedicated launcher page directly (no Shell at the window root).
		return new Window(new NavigationPage(new RuntimeAppearanceTestLauncherPage()));
	}
}
