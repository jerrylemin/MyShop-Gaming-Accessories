using Microsoft.UI.Xaml;
using ProjectTest.ViewModels;

namespace ProjectTest.Views;

public sealed partial class DatabaseSetupWindow : Window
{
    public DatabaseSetupWindow(string errorMessage)
    {
        ViewModel = new DatabaseSetupViewModel(
            Services.DatabaseOptionsProvider.GetConfiguredConnectionString(),
            errorMessage,
            App.Current.ConfigureDatabaseAsync);

        InitializeComponent();
        RootGrid.DataContext = ViewModel;
    }

    public DatabaseSetupViewModel ViewModel { get; }
}
