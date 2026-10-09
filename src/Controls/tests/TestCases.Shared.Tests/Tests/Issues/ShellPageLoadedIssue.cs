using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class ShellPageLoadedIssue : _IssuesUITest
	{
		public override string Issue => "For pages used in FlyoutItem/Tab/ShellContent the Page.Loaded event is called erratically";

		public ShellPageLoadedIssue(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void PageLoadedShouldOnlyFireOncePerNavigation()
		{
			// Test scenario 1: Navigate from Page 1 to Page 2 should only fire Loaded once for Page 2
			App.WaitForElement("Page1Label");
			
			// Initially Page 1 should be loaded once
			var loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page1 Loaded Count: 1"));
			
			// Navigate to Page 2
			App.Tap("NavigateToOtherButton");
			App.WaitForElement("Page2Label");
			
			// Page 2 should be loaded exactly once
			loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page2 Loaded Count: 1"));
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void PageLoadedShouldNotFireForIntermediatePages()
		{
			// Test scenario 2: Navigate from Page 1 to Page 3 (skipping Page 2)
			// Page 2 should not have its Loaded event fired
			App.WaitForElement("Page1Label");
			
			// Navigate directly to Page 3
			App.Tap("NavigateToAnotherButton");
			App.WaitForElement("Page3Label");
			
			// Page 3 should be loaded exactly once
			var loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page3 Loaded Count: 1"));
			
			// Navigate back to check Page 2 was not loaded
			App.Tap("NavigateToOtherButton");
			App.WaitForElement("Page2Label");
			
			// Page 2 should only be loaded once (when we navigate to it), not when we passed through
			loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page2 Loaded Count: 1"));
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void PageLoadedShouldNotFireMultipleTimesForSamePage()
		{
			// Test scenario 3: Navigate to Page 2, then back to Page 1, then to Page 2 again
			// Each page should only fire Loaded once per navigation
			App.WaitForElement("Page1Label");
			
			// Navigate to Page 2
			App.Tap("NavigateToOtherButton");
			App.WaitForElement("Page2Label");
			
			// Navigate back to Page 1
			App.Tap("NavigateToMainButton");
			App.WaitForElement("Page1Label");
			
			// Page 1 should now show it was loaded twice (initial + return navigation)
			var loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page1 Loaded Count: 2"));
			
			// Navigate to Page 2 again
			App.Tap("NavigateToOtherButton");
			App.WaitForElement("Page2Label");
			
			// Page 2 should now show it was loaded twice
			loadedCountLabel = App.WaitForElement("LoadedCountLabel");
			Assert.That(loadedCountLabel.GetText(), Is.EqualTo("Page2 Loaded Count: 2"));
		}
	}
}