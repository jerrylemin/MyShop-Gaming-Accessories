using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

    private async void BrowseImage1Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Image1 = await PickAndStoreImageAsync(ViewModel.Image1);
    }

    private async void BrowseImage2Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Image2 = await PickAndStoreImageAsync(ViewModel.Image2);
    }

    private async void BrowseImage3Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Image3 = await PickAndStoreImageAsync(ViewModel.Image3);
    }

    private async Task<string> PickAndStoreImageAsync(string currentPath)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        if (App.Current.ActiveWindow is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return currentPath;
        }

        var productImagesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProductEditorImages", CreationCollisionOption.OpenIfExists);
        var extension = Path.GetExtension(file.Name);
        var destinationName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
        var copiedFile = await file.CopyAsync(productImagesFolder, destinationName, NameCollisionOption.ReplaceExisting);
        return copiedFile.Path;
    }
}
