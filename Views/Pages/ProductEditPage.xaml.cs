using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class ProductEditPage : Page
{
    public ProductEditPage()
    {
        ViewModel = new ProductEditViewModel(App.Current.Services.ProductRepository, App.Current.Services.CategoryRepository);
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.Saved += (_, _) =>
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                App.Current.Services.NavigationService.Navigate("Products");
            }
        };
    }

    public ProductEditViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var productId = e.Parameter is int id ? id : 0;
        await ViewModel.LoadAsync(productId);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
        else
        {
            App.Current.Services.NavigationService.Navigate("Products");
        }
    }
}
