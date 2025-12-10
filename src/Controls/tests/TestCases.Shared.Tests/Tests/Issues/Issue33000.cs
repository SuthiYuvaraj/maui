using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33000 : _IssuesUITest
{
    public override string Issue => "Email.ComposeAsync - problem with empty body and subject on Spark email application";

    public Issue33000(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.ManualReview)]
    public void EmailComposerOpensWithSubjectAndBody()
    {
        // This test requires MANUAL VERIFICATION in email apps (Spark, Gmail, etc.):
        // BEFORE FIX: Subject and Body were EMPTY in Spark app when no attachments
        // AFTER FIX: Subject and Body should be populated correctly
        // 
        // Expected values when tapping "Compose Email (Subject + Body)":
        //   Subject: "Test from .NET MAUI - Issue 33000"
        //   Body: "This email tests that subject and body are correctly passed to Spark and other email apps.\n\nIf you can read this, the fix works!"

        App.WaitForElement("StatusLabel");

        // Verify all test buttons are present
        App.WaitForElement("ComposeBasicEmailButton");
        App.WaitForElement("ComposeFullEmailButton");
        App.WaitForElement("ComposeHtmlEmailButton");
        App.WaitForElement("ComposeEmailWithCcBccButton");

        // Test basic email composition (Subject + Body only)
        App.Tap("ComposeBasicEmailButton");

        // Give time for email app to open
        System.Threading.Thread.Sleep(2000);

        // Verify status updated to success
        var status = App.WaitForElement("StatusLabel").GetText();
        Assert.That(status, Does.Contain("✅"), "Status should show success after tapping compose button");

        // MANUAL VERIFICATION REQUIRED:
        // Check that the email composer opened with subject and body populated
        // Test in multiple email apps, especially Spark
    }
}
