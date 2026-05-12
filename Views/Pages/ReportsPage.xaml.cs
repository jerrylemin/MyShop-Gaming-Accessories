using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class ReportsPage : Page
{
    private bool _initialLoadQueued;
    private CancellationTokenSource? _navigationLoadCts;

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

        _navigationLoadCts?.Cancel();
        _navigationLoadCts = new CancellationTokenSource();
        var token = _navigationLoadCts.Token;

        if (_initialLoadQueued)
        {
            _ = ViewModel.InitializeAsync(token);
            return;
        }

        _initialLoadQueued = true;
        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, async () =>
        {
            await ViewModel.InitializeAsync(token);
        });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _navigationLoadCts?.Cancel();
        ViewModel.CancelLoading();
        base.OnNavigatedFrom(e);
    }
}
