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
            // Wait for BlazorWebView to load - wait for the title label from XAML
            App.WaitForElement("TitleLabel");
            
            // Wait for Home page content to appear in BlazorWebView
            App.WaitForElement("Home Page");
            
            // Click "Go to Counter" link
            App.Tap("Go to Counter");
            
            // Wait for Counter page to load
            App.WaitForElement("Counter");
            
            // Perform swipe from left edge to simulate back gesture
            // This should navigate back within BlazorWebView instead of closing the app
            App.SwipeLeftToRight(swipePercentage: 0.10, swipeSpeed: 500);
            
            // Verify we're back on the Home page (BlazorWebView navigated back, app did NOT close)
            App.WaitForElement("Home Page");
            
            // Verify the "Go to Counter" link is visible again
            App.WaitForElement("Go to Counter");
        }
    }
}
