using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31150 : _IssuesUITest
	{
		public Issue31150(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Mismatch between WidthRequest/HeightRequest and Measure results";

		[Test]
		[Category(UITestCategories.Layout)]
		public void WidthRequestHeightRequestShouldMatchMeasureConstraints()
		{
			App.WaitForElement("Issue31150TestLayout");
			App.Tap("Issue31150TriggerButton");
			var testStatus = App.WaitForElement("Issue31150TestStatus");
			var statusText = testStatus.GetText();
			Assert.That(statusText, Does.Contain("PASSED"), 
				$"Layout constraints should match WidthRequest/HeightRequest, not screen size. Status: {statusText}");
			var layoutConstraintsResult = App.WaitForElement("Issue31150LayoutConstraintsResult");
			var layoutText = layoutConstraintsResult.GetText();
			Assert.That(layoutText, Does.Contain("W=200") | Does.Contain("W=2"), 
				$"Width constraint should be close to 200, not screen width. Got: {layoutText}");
			Assert.That(layoutText, Does.Contain("H=150") | Does.Contain("H=1"), 
				$"Height constraint should be close to 150, not screen height. Got: {layoutText}");
		}
		
	}
}