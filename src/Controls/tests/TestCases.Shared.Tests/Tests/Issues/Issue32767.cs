using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue32767 : _IssuesUITest
    {
        public Issue32767(TestDevice testDevice) : base(testDevice)
        {
        }

        public override string Issue => "Back navigation different between .net 9 and .net 10 blazor hybrid";

        [Test]
        [Category(UITestCategories.WebView)]
        public void BlazorWebViewBackNavigationNavigatesWithinWebViewFirst()
        {
            // Wait for BlazorWebView to load
            App.WaitForElement("TitleLabel");

            // Give BlazorWebView time to fully initialize
            Task.Delay(2000).Wait();

            // Navigate to Page 2
            App.WaitForElement("NavigateToPage2Button");
            App.Tap("NavigateToPage2Button");

            // Wait for Page 2 to load
            Task.Delay(1000).Wait();
            App.WaitForElement("NavigateToPage3Button");

            // Navigate to Page 3
            App.Tap("NavigateToPage3Button");

            // Wait for Page 3 to load
            Task.Delay(1000).Wait();
            App.WaitForElement("Page3Content");

            // Press back - should navigate to Page 2 within BlazorWebView
            App.Back();
            Task.Delay(1000).Wait();

            // Verify we're on Page 2 (within BlazorWebView, not popped to previous MAUI page)
            App.WaitForElement("NavigateToPage3Button");

            // Press back again - should navigate to Page 1 within BlazorWebView
            App.Back();
            Task.Delay(1000).Wait();

            // Verify we're on Page 1 (still within BlazorWebView)
            App.WaitForElement("NavigateToPage2Button");

            // Press back one more time - now it should pop the ContentPage
            // since BlazorWebView has no more history
            App.Back();
            Task.Delay(1000).Wait();

            // Verify we've navigated away from the test page
            // (the title label should no longer be present)
            App.WaitForNoElement("TitleLabel");
        }
    }
}
