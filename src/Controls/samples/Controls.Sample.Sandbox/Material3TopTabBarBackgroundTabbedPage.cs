namespace Maui.Controls.Sample;

/// <summary>
/// Dedicated TabbedPage root for scenario 6b: verify TabbedPageManager's Brush-based
/// BarBackground apply/reset (RefreshBarBackground) on Top tab placement
/// (TabLayout code path) - the path that previously had no restore-on-clear handling.
/// </summary>
public class Material3TopTabBarBackgroundTabbedPage : TabbedPage
{
	public Material3TopTabBarBackgroundTabbedPage()
	{
		Title = "Bar Background Test (Top Tab)";

		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(
			this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Top);

		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 1" });
		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 2" });
		Children.Add(new Material3BarBackgroundContentPage { Title = "Tab 3" });
	}
}
