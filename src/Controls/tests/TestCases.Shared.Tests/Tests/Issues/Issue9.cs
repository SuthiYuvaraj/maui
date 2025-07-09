using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9 : _IssuesUITest
{
	public Issue9(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "DatePicker Does Not Update Its Format When the Culture Is Changed at Runtime";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerFormatUpdatesWhenCultureChanges()
	{
		App.WaitForElement("TestDatePicker");
		
		// Get the initial date format
		var initialDateFormat = GetDatePickerText();
		
		// Change culture to German
		App.Tap("German (Germany)");
		App.WaitForElement("TestDatePicker");
		
		// Wait for culture change to propagate
		App.WaitForNoElement("No tests run yet.", timeout: TimeSpan.FromSeconds(5));
		
		// Get the new date format after culture change
		var germanDateFormat = GetDatePickerText();
		
		// The formats should be different
		Assert.That(germanDateFormat, Is.Not.EqualTo(initialDateFormat), 
			"DatePicker format should change when culture changes");
		
		// Change culture to French
		App.Tap("French (France)");
		App.WaitForElement("TestDatePicker");
		
		// Wait for culture change to propagate
		System.Threading.Thread.Sleep(500);
		
		// Get the French date format
		var frenchDateFormat = GetDatePickerText();
		
		// The French format should be different from German
		Assert.That(frenchDateFormat, Is.Not.EqualTo(germanDateFormat), 
			"DatePicker format should change when culture changes to French");
		
		// Verify we can still interact with the DatePicker
		App.Tap("TestDatePicker");
		// On some platforms, this opens a date picker dialog
		// The test passes if no exception is thrown
	}

	private string GetDatePickerText()
	{
		var datePicker = App.FindElement("TestDatePicker");
		return datePicker.GetText();
	}
}