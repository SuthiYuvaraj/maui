namespace Maui.Controls.Sample;

public partial class RuntimeAppearanceTestLauncherPage : ContentPage
{
	public RuntimeAppearanceTestLauncherPage()
	{
		InitializeComponent();
	}

	async void OnTabbedPageTestClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new RuntimeTabbedPageTest());
	}

	void OnShellTabsTestClicked(object sender, EventArgs e)
	{
		// Shell must be the Window's root page, so swap it in directly instead
		// of pushing it onto the current NavigationPage stack.
		if (Application.Current?.Windows.Count > 0)
			Application.Current.Windows[0].Page = new RuntimeShellTabsTestShell();
	}

	void OnShellTopTabsTestClicked(object sender, EventArgs e)
	{
		if (Application.Current?.Windows.Count > 0)
			Application.Current.Windows[0].Page = new RuntimeShellTopTabsTestShell();
	}

	void OnShellFlyoutTestClicked(object sender, EventArgs e)
	{
		if (Application.Current?.Windows.Count > 0)
			Application.Current.Windows[0].Page = new RuntimeShellFlyoutTestShell();
	}

	void OnShellDisabledColorTestClicked(object sender, EventArgs e)
	{
		if (Application.Current?.Windows.Count > 0)
			Application.Current.Windows[0].Page = new RuntimeShellDisabledColorTestShell();
	}

	async void OnToolbarTestClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new RuntimeToolbarTestPage());
	}
}
