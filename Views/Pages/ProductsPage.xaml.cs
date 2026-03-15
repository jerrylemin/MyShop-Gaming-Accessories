using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using ProjectTest.ViewModels;
using WinRT.Interop;

namespace ProjectTest.Views.Pages;

public sealed partial class ProductsPage : Page
{
    public ProductsPage()
    {
        ViewModel = new ProductsViewModel(
            App.Current.Services.ProductRepository,
            App.Current.Services.CategoryRepository,
            App.Current.Services.ExcelProductImportService,
            App.Current.Services.SettingsService);
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += ProductsPage_Loaded;
        ViewModel.EditRequested += ViewModel_EditRequested;
    }

    public ProductsViewModel ViewModel { get; }

    private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ProductsPage_Loaded;
        await ViewModel.LoadAsync(1);
    }

    private void ViewModel_EditRequested(object? sender, int? productId)
    {
        App.Current.Services.NavigationService.Navigate("ProductEdit", productId ?? 0, persist: false);
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete product",
            Content = "Delete the selected gaming accessory from the catalog? Products with order history cannot be removed.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteSelectedAsync();
        }
    }

    private async void ImportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".xlsx");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        if (App.Current.ActiveWindow is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        await ViewModel.ImportFromExcelAsync(file.Path);
    }
}
