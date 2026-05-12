using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class ReportsPage : Page
{
    private bool _initialLoadQueued;

    public ReportsPage()
    {
        ViewModel = new ReportsViewModel(App.Current.Services.ReportingService);
        InitializeComponent();
        DataContext = ViewModel;
    }

    public ReportsViewModel ViewModel { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_initialLoadQueued)
        {
            _ = ViewModel.InitializeAsync();
            return;
        }

        _initialLoadQueued = true;
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, async () =>
        {
            await ViewModel.InitializeAsync();
        });
    }
}
