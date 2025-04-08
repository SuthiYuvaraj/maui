
using TextInputLayoutSample.ViewModels;

namespace TextInputLayoutSample;

public partial class HomePage : ContentPage
{
    private AssessmentsViewModel ViewModel => (AssessmentsViewModel)BindingContext;

    public HomePage()
    {
        InitializeComponent();
    }

    //protected override async void OnAppearing()
    //{
    //    ViewModel.IsBusy = true;
    //    ViewModel.CurrentState = "Loading";

    //    await Task.Delay(600);

    //    ViewModel.IsBusy = false;
    //    ViewModel.CurrentState = "Success";
    //}

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    ViewModel.SearchTextQuery = string.Empty;
    //    ViewModel.CurrentState = "Loading";
    //}

    private void OnNavigateToDetailsClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new DetailsPage());
    }
}