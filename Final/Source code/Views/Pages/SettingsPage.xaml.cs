using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;
using System;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;

namespace ProjectTest.Views.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        ViewModel = new SettingsViewModel(
            App.Current.Services.SettingsService,
            App.Current.Services.AuthenticationService,
            App.Current.Services.LicenseService,
            App.Current.Services.BackupRestoreService,
            App.Current.Services.LlmAssistantService);
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += SettingsPage_Loaded;
    }

    public SettingsViewModel ViewModel { get; }

    private async void SettingsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= SettingsPage_Loaded;
        await ViewModel.LoadAsync();
    }

    private async void BrowsePostgreSqlToolsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));

        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            ViewModel.PostgreSqlToolsPath = folder.Path;
        }
    }

    private async void BrowseBackupButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"myshop-backup-{DateTime.Today:yyyyMMdd}"
        };
        picker.FileTypeChoices.Add("PostgreSQL dump", [".dump"]);
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));

        var file = await picker.PickSaveFileAsync();
        if (file is not null)
        {
            ViewModel.BackupPath = file.Path;
        }
    }

    private async void BrowseRestoreButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add(".dump");
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            ViewModel.RestorePath = file.Path;
        }
    }

    private async void InstallPostgreSqlButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://www.postgresql.org/download/windows/"));
    }
}
