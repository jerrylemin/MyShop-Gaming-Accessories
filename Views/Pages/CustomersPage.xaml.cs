using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class CustomersPage : Page
{
    public CustomersPage()
    {
        ViewModel = new CustomersViewModel(App.Current.Services.CustomerRepository);
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += CustomersPageLoaded;
    }

    public CustomersViewModel ViewModel { get; }

    private async void CustomersPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CustomersPageLoaded;
        await ViewModel.LoadAsync();
    }
}
