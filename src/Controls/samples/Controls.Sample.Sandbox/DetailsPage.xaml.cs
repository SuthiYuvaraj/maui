namespace TextInputLayoutSample;

public partial class DetailsPage : ContentPage
{
    public DetailsPage()
    {
        InitializeComponent();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
        var newTheme = currentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = newTheme;
        }
    }
}