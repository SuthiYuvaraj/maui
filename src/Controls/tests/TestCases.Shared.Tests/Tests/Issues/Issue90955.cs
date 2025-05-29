using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue90955 : _IssuesUITest
	{
		public override string Issue => "RadioButton CharacterSpacing Property Does Not Apply on Windows Platforms";

		public Issue90955(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.RadioButton)]
		public void CharacterSpacingShouldApplyon()
		{
			App.WaitForElement("Issue90955RadioButton");
			App.WaitForElement("Issue90955Label");
			VerifyScreenshot();
		}
	}
}
