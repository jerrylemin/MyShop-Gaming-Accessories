using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.Helpers;
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
        var selectedPath = await PickAndStoreImageAsync(ViewModel.Image1, "Image 1");
        ApplySelectedImage(selectedPath, value => ViewModel.Image1 = value, Image1TextBox, Image1Preview);
    }

    private async void BrowseImage2Button_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = await PickAndStoreImageAsync(ViewModel.Image2, "Image 2");
        ApplySelectedImage(selectedPath, value => ViewModel.Image2 = value, Image2TextBox, Image2Preview);
    }

    private async void BrowseImage3Button_Click(object sender, RoutedEventArgs e)
    {
        var selectedPath = await PickAndStoreImageAsync(ViewModel.Image3, "Image 3");
        ApplySelectedImage(selectedPath, value => ViewModel.Image3 = value, Image3TextBox, Image3Preview);
    }

    private async Task<string> PickAndStoreImageAsync(string currentPath, string label)
    {
        try
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
                ViewModel.StatusMessage = $"{label} was not changed.";
                return currentPath;
            }

            var storedPath = await CopyImageToAppStorageAsync(file);
            ViewModel.StatusMessage = $"{label} selected and copied into app storage. Click Save Product to save it to the database.";
            return storedPath;
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Could not select {label.ToLowerInvariant()}: {ex.GetBaseException().Message}";
            return currentPath;
        }
    }

    private async Task<string> CopyImageToAppStorageAsync(StorageFile file)
    {
        var productImagesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            "ProductEditorImages",
            CreationCollisionOption.OpenIfExists);

        var extension = Path.GetExtension(file.Name);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        var productKey = string.IsNullOrWhiteSpace(ViewModel.SKU) ? ViewModel.Name : ViewModel.SKU;
        var safeProductKey = MakeSafeFileName(productKey);
        var destinationName = $"{safeProductKey}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";

        var copiedFile = await file.CopyAsync(
            productImagesFolder,
            destinationName,
            NameCollisionOption.GenerateUniqueName);

        return $"ms-appdata:///local/ProductEditorImages/{copiedFile.Name}";
    }

    private static string MakeSafeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "product";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(value.Trim().Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray())
            .Replace(' ', '_');

        return safeName.Length > 40 ? safeName[..40] : safeName;
    }

    private static void ApplySelectedImage(string path, Action<string> setViewModelPath, TextBox textBox, Image preview)
    {
        setViewModelPath(path);
        textBox.Text = path;
        preview.Source = ImageSourceHelper.ToBitmap(path);
    }
}