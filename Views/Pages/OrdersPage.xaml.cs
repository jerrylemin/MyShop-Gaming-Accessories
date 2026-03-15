using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class OrdersPage : Page
{
    public OrdersPage()
    {
        ViewModel = new OrdersViewModel(App.Current.Services.OrderRepository, App.Current.Services.ProductRepository);
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OrdersPage_Loaded;
    }

    public OrdersViewModel ViewModel { get; }

    private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OrdersPage_Loaded;
        await ViewModel.LoadAsync();
    }

    private async void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedOrder is not null)
        {
            await ViewModel.LoadOrderAsync(ViewModel.SelectedOrder.Id);
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete order",
            Content = "Delete the current order and restore its stock allocations?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteSelectedAsync();
        }
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is OrderLineViewModel line)
        {
            ViewModel.RemoveLine(line);
        }
    }
}
