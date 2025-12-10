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
        App.WaitForElement("StatusLabel");
        App.WaitForElement("ComposeBasicEmailButton");
        App.Tap("ComposeBasicEmailButton");
        VerifyScreenshot();
    }
}
