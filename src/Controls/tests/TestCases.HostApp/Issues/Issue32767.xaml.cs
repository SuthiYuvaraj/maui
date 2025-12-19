using Microsoft.Maui.Controls.CustomAttributes;
using Controls.TestCases.HostApp.Issues.Issue32767Components;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 32767, "Back navigation different between .net 9 and .net 10 blazor hybrid", PlatformAffected.Android)]
    public partial class Issue32767 : ContentPage
    {
        public Issue32767()
        {
            InitializeComponent();
        }
    }
}
