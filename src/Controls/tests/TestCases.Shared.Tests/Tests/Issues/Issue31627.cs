using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31627 : _IssuesUITest
{
	public override string Issue => "CollectionView throws LayoutCycleException on Windows";

	public Issue31627(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ManyCollectionViewsShouldNotCauseLayoutCycle()
	{
		// This bug is specific to Windows; only run there.
		this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

		// If the app crashes with a LayoutCycleException while laying out the many
		// CollectionViews, we'll never reach this point. Wait for the Loaded handler
		// to set "Success" text, avoiding a race with element existence.
		App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(30));
	}
}
