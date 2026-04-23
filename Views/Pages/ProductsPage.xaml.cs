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
        ViewModel.CategoryEditRequested += ViewModel_CategoryEditRequested;
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

    private async void ViewModel_CategoryEditRequested(object? sender, Models.CategoryListItem? category)
    {
        var nameBox = new TextBox
        {
            PlaceholderText = "Category name",
            Text = category?.Name ?? string.Empty
        };
        var descriptionBox = new TextBox
        {
            AcceptsReturn = true,
            Height = 120,
            PlaceholderText = "Category description",
            Text = category?.Description ?? string.Empty,
            TextWrapping = TextWrapping.Wrap
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = "Name" });
        panel.Children.Add(nameBox);
        panel.Children.Add(new TextBlock { Text = "Description" });
        panel.Children.Add(descriptionBox);

        var dialog = new ContentDialog
        {
            Title = category is null ? "Add category" : "Edit category",
            Content = panel,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.SaveCategoryAsync(category?.Id ?? 0, nameBox.Text, descriptionBox.Text);
        }
    }
}
