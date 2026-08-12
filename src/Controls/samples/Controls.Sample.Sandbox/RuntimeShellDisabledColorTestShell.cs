namespace Maui.Controls.Sample;

public class RuntimeShellDisabledColorTestShell : Shell
{
    readonly Tab _disabledTab;

    public RuntimeShellDisabledColorTestShell()
    {
        Title = "Disabled Color Test";

        var tabBar = new TabBar();
        tabBar.Items.Add(CreateTab("Tab 1", new RuntimeShellDisabledColorTestPage(this)));

        _disabledTab = CreateTab("Tab 2 (disabled)", new ContentPage
        {
            Title = "Tab 2 (disabled)",
            Content = new Label
            {
                Text = "Tab 2 content",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        });
        _disabledTab.IsEnabled = false;
        tabBar.Items.Add(_disabledTab);

        tabBar.Items.Add(CreateTab("Tab 3", new ContentPage
        {
            Title = "Tab 3",
            Content = new Label
            {
                Text = "Tab 3 content",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            }
        }));

        Items.Add(tabBar);
    }

    public void ToggleDisabledTab()
    {
        _disabledTab.IsEnabled = !_disabledTab.IsEnabled;
    }

    static Tab CreateTab(string title, Page content)
    {
        var tab = new Tab { Title = title };
        tab.Items.Add(new ShellContent
        {
            Title = title,
            Content = content,
        });
        return tab;
    }
}

public class RuntimeShellDisabledColorTestPage : ContentPage
{
    readonly RuntimeShellDisabledColorTestShell _shell;

    public RuntimeShellDisabledColorTestPage(RuntimeShellDisabledColorTestShell shell)
    {
        _shell = shell;
        Title = "Disabled Color Test";

        var setDisabledColorButton = new Button { Text = "Set explicit DisabledColor (Orange)" };
        setDisabledColorButton.Clicked += (_, _) => Shell.SetDisabledColor(_shell, Colors.Orange);

        var setTabBarDisabledColorButton = new Button { Text = "Set explicit TabBarDisabledColor (Red)" };
        setTabBarDisabledColorButton.Clicked += (_, _) => Shell.SetTabBarDisabledColor(_shell, Colors.Red);

        var resetButton = new Button { Text = "Reset DisabledColor/TabBarDisabledColor (null, native)" };
        resetButton.Clicked += (_, _) =>
        {
            _shell.ClearValue(Shell.DisabledColorProperty);
            _shell.ClearValue(Shell.TabBarDisabledColorProperty);
        };

        var toggleButton = new Button { Text = "Toggle Tab 2 enabled/disabled" };
        toggleButton.Clicked += (_, _) => _shell.ToggleDisabledTab();

        var backButton = new Button { Text = "Back to runtime appearance test menu" };
        backButton.Clicked += (_, _) =>
        {
            if (Application.Current?.Windows.Count > 0)
                Application.Current.Windows[0].Page = new NavigationPage(new RuntimeAppearanceTestLauncherPage());
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 22,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Scenario 4: Bottom tab disabled color (Shell)",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                    },
                    new Label
                    {
                        Text = "'Tab 2 (disabled)' starts disabled so ShellBottomNavViewAppearanceTracker's disabled-color code path (M2 hardcoded AColor.Gray vs. M3 captured native _originalDisabledColor) renders immediately.",
                    },
                    setDisabledColorButton,
                    setTabBarDisabledColorButton,
                    resetButton,
                    new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 2) },
                    toggleButton,
                    new BoxView { HeightRequest = 1, Color = Colors.Gray, Margin = new Thickness(0, 2) },
                    new Label
                    {
                        Text = "What to check visually:",
                        FontAttributes = FontAttributes.Bold,
                    },
                    new Label { Text = "- With no explicit color set, the disabled tab uses a theme-correct native gray on M3 (not a stale/wrong hardcoded color) and the original hardcoded gray/black on M2 (unchanged)." },
                    new Label { Text = "- Setting an explicit DisabledColor/TabBarDisabledColor immediately recolors 'Tab 2 (disabled)' on both M2 and M3." },
                    new Label { Text = "- Resetting back to null restores the correct default (native on M3, hardcoded on M2) rather than leaving the stale explicit color." },
                    new Label { Text = "- Toggling Tab 2 enabled/disabled re-applies the same displayed color consistently each time." },
                    backButton,
                }
            }
        };
    }
}