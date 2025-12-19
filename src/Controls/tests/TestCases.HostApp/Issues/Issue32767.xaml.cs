using Microsoft.Maui.Controls.CustomAttributes;
using Controls.TestCases.HostApp.Issues.Issue32767Components;
using Microsoft.AspNetCore.Components.WebView;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 32767, "Back navigation different between .net 9 and .net 10 blazor hybrid", PlatformAffected.Android)]
    public partial class Issue32767 : ContentPage
    {
        public Issue32767()
        {
            InitializeComponent();
        }


        private void OnBlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
        {
#if ANDROID
    var platformWebView = e.WebView as Android.Webkit.WebView;
    System.Diagnostics.Debug.WriteLine("[BlazorWebView] Handler created? " + (platformWebView != null));

    if (platformWebView != null)
    {
        System.Diagnostics.Debug.WriteLine("[BlazorWebView] Current URL: " + platformWebView.Url);
        // If assets are OK, this typically becomes something like https://0.0.0.0/
       }
#endif

        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                using var s = await FileSystem.OpenAppPackageFileAsync("wwwroot/index.html");
                System.Diagnostics.Debug.WriteLine("[BlazorWebView] index.html FOUND");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BlazorWebView] index.html NOT FOUND: {ex.Message}");
            }
        }

    }
}
