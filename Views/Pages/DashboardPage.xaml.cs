using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        ViewModel = new DashboardViewModel(App.Current.Services.DashboardService);
        InitializeComponent();
        DataContext = ViewModel;
    }

    public DashboardViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        RefreshPage();
    }

    private async void RefreshPage()
    {
        await ViewModel.RefreshAsync();
    }
}
