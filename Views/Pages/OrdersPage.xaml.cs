using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectTest.Models;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class OrdersPage : Page
{
    public OrdersPage()
    {
        ViewModel = new OrdersViewModel(
            App.Current.Services.OrderRepository,
            App.Current.Services.ProductRepository,
            App.Current.Services.SettingsService,
            App.Current.Services.CustomerRepository,
            App.Current.Services.PromotionRepository,
            App.Current.Services.CurrentUserService,
            App.Current.Services.InvoiceExportService);
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.CreateCustomerRequested += ViewModel_CreateCustomerRequested;
        Loaded += OrdersPage_Loaded;
        SizeChanged += OrdersPage_SizeChanged;
    }

    public OrdersViewModel ViewModel { get; }

    private void OrdersPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var isNarrow = e.NewSize.Width < 920;
        OrdersWorkspaceGrid.ColumnDefinitions[0].Width = isNarrow ? new GridLength(0) : new GridLength(1.05, GridUnitType.Star);
        OrdersWorkspaceGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        OrdersWorkspaceGrid.Children[0].Visibility = isNarrow ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OrdersPage_Loaded;
        await ViewModel.LoadAsync(1);
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

    private async void ViewModel_CreateCustomerRequested(object? sender, CreateCustomerRequestedEventArgs e)
    {
        var nameBox = new TextBox
        {
            Header = "Customer name",
            PlaceholderText = "Enter customer name"
        };
        var phoneBox = new TextBox
        {
            Header = "Phone",
            Text = e.Phone,
            IsReadOnly = true
        };
        var emailBox = new TextBox
        {
            Header = "Email (optional)",
            PlaceholderText = "customer@example.com"
        };
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock
        {
            Text = "This phone number is not in Customers yet. Add the customer now to save this order.",
            TextWrapping = TextWrapping.WrapWholeWords
        });
        panel.Children.Add(nameBox);
        panel.Children.Add(phoneBox);
        panel.Children.Add(emailBox);

        var dialog = new ContentDialog
        {
            Title = "New customer",
            Content = panel,
            PrimaryButtonText = "Save customer",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            e.SetResult(null);
            return;
        }

        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            var validationDialog = new ContentDialog
            {
                Title = "Customer name required",
                Content = "Enter a customer name before saving the order.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await validationDialog.ShowAsync();
            e.SetResult(null);
            return;
        }

        var saveResult = await App.Current.Services.CustomerRepository.SaveAsync(new Customer
        {
            Name = nameBox.Text.Trim(),
            Phone = e.Phone,
            Email = emailBox.Text.Trim()
        });

        if (!saveResult.Success)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Customer was not saved",
                Content = saveResult.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await errorDialog.ShowAsync();
            e.SetResult(null);
            return;
        }

        e.SetResult(await App.Current.Services.CustomerRepository.GetByIdAsync(saveResult.Value));
    }
}
