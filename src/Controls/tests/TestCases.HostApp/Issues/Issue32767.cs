using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Controls.TestCases.HostApp.Issues.Issue32767Components;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 32767, "Back navigation different between .net 9 and .net 10 blazor hybrid", PlatformAffected.Android)]
    public class Issue32767 : ContentPage
    {
        public Issue32767()
        {
            AutomationId = "Issue32767HomePage";
            Title = "Issue32767";

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            var titleLabel = new Label
            {
                Text = "BlazorWebView Back Navigation Test",
                Margin = new Thickness(10),
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                AutomationId = "TitleLabel"
            };
            Grid.SetRow(titleLabel, 0);

            var blazorWebView = new BlazorWebView
            {
                HostPage = "wwwroot/index.html"
            };
            blazorWebView.RootComponents.Add(new RootComponent
            {
                Selector = "#app",
                ComponentType = typeof(Main)
            });
            Grid.SetRow(blazorWebView, 1);

            grid.Add(titleLabel);
            grid.Add(blazorWebView);

            Content = grid;
        }
    }
}
