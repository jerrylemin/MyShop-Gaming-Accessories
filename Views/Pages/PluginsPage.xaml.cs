using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class PluginsPage : Page
{
    public PluginsPage()
    {
        ViewModel = new PluginsViewModel(App.Current.Services.PluginService);
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += PluginsPage_Loaded;
    }

    public PluginsViewModel ViewModel { get; }

    private async void PluginsPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PluginsPage_Loaded;
        await ViewModel.LoadAsync();
    }
}
