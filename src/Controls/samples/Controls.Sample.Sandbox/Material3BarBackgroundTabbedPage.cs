namespace Maui.Controls.Sample;

/// <summary>
/// Dedicated TabbedPage root for scenario 6a: verify TabbedPageManager's Brush-based
/// BarBackground apply/reset (RefreshBarBackground) on Bottom tab placement
/// (BottomNavigationView code path).
/// </summary>
public class Material3BarBackgroundTabbedPage : TabbedPage
{
	public Material3BarBackgroundTabbedPage()
	{
		Title = "Bar Background Test (Bottom Tab)";

		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
			this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 1" });
		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 2" });
		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 3" });
	}
}
