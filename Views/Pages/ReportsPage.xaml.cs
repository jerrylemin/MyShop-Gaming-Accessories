using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class ReportsPage : Page
{
    public ReportsPage()
    {
        ViewModel = new ReportsViewModel(App.Current.Services.ReportingService);
        InitializeComponent();
        DataContext = ViewModel;
    }

    public ReportsViewModel ViewModel { get; }

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
