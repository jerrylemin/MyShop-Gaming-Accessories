using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

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
            App.Current.Services.PluginService,
            App.Current.Services.GraphQlPosService);
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
}
