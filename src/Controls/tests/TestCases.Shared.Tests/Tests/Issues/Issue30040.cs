using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30040 : _IssuesUITest
{
	public Issue30040(TestDevice device) : base(device) { }

	public override string Issue => "Keyboard navigation does not work with RadioButtons with redefined appearance";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void KeyboardNavigationWorksWithRadioButtonControlTemplate()
	{
		if (Device != TestDevice.Android)
			Assert.Ignore("Keyboard navigation via DPAD is Android-specific for this issue");

		App.WaitForElement("Radio1");

		// Tap Radio1 — selects it and gives keyboard focus (FocusableInTouchMode = true)
		App.Tap("Radio1");
		Assert.That(App.FindElement("SelectedLabel").GetText(), Is.EqualTo("Selected: Option 1"),
			"Radio1 should be selected after tap");

		// DPAD_DOWN moves focus to Radio2 and selects it (MoveAndSelectSibling)
		App.SendKeys(20); // KEYCODE_DPAD_DOWN

		Assert.That(App.FindElement("SelectedLabel").GetText(), Is.EqualTo("Selected: Option 2"),
			"Radio2 should be selected after DPAD_DOWN");
	}
}
