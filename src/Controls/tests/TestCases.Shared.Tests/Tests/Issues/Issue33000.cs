using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33000 : _IssuesUITest
{
    public override string Issue => "Email.ComposeAsync - problem with empty body and subject on Spark email application";

    public Issue33000(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Email)]
    public void Issue33000Test()
    {
        // Wait for page to load
        App.WaitForElement("ComposeBasicEmailButton");

        // Verify all test buttons are present
        App.WaitForElement("ComposeFullEmailButton");
        App.WaitForElement("ComposeHtmlEmailButton");
        App.WaitForElement("ComposeEmailWithCcBccButton");

        // Verify status label exists
        var statusLabel = App.WaitForElement("StatusLabel");
        Assert.That(statusLabel, Is.Not.Null, "Status label should be present");

        // Test basic email composition (Subject + Body only)
        App.Tap("ComposeBasicEmailButton");

        // Give the system time to launch email app
        Task.Delay(1000).Wait();

        // Verify status updated to success
        var status = App.WaitForElement("StatusLabel").GetText();
        Assert.That(status, Does.Contain("✅"), "Status should show success after tapping compose button");

        // Note: Manual verification needed in email app to confirm subject and body appear
        // This automated test verifies the API doesn't crash and the compose action is triggered
    }
}
