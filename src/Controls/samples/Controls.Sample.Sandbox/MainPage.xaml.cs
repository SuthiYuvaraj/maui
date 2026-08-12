namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}
}

public class CustomTabbedPage : TabbedPage
{
	public CustomTabbedPage()
	{
		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Top);
		for (int i = 1; i <= 3; i++)
		{
			var contentPage = new MainPage
			{
				Title = $"Tab {i}",

			};
			var navPage = new NavigationPage(contentPage)
			{
				Title = $"Tab {i}",
			};
			this.Children.Add(navPage);
		}
	}

}