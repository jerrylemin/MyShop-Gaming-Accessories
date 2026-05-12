using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly ReportingService _reportingService;
    private CancellationTokenSource? _activeLoadCts;
    private int _loadVersion;
    private bool _isLoading;
    private bool _hasLoadedSnapshot;
    private DateTime? _loadedFromDate;
    private DateTime? _loadedToDate;
    private DateTimeOffset _fromDate = new(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
    private DateTimeOffset _toDate = new(DateTime.Today);
    private string _rangeLabel = "Reports are ready.";
    private string _statusMessage = "Reports will load after the page appears.";
    private decimal _totalRevenue;
    private decimal _totalProfit;
    private ObservableCollection<ChartPoint> _revenueByDay = [];
    private ObservableCollection<ChartPoint> _profitByDay = [];
    private ObservableCollection<BarChartItem> _revenueByWeek = [];
    private ObservableCollection<BarChartItem> _profitByWeek = [];
    private ObservableCollection<BarChartItem> _revenueByMonth = [];
    private ObservableCollection<BarChartItem> _profitByMonth = [];
    private ObservableCollection<BarChartItem> _revenueByYear = [];
    private ObservableCollection<BarChartItem> _profitByYear = [];
    private ObservableCollection<BarChartItem> _productSalesByRange = [];
    private ObservableCollection<PieChartItem> _productSalesShare = [];
    private ObservableCollection<SalesCommissionSnapshot> _salesCommissions = [];
    private ObservableCollection<MlInsight> _mlInsights = [];
    private string _assistantSummary = "Assistant summary will appear after reports finish loading.";

    public ReportsViewModel(ReportingService reportingService)
    {
        _reportingService = reportingService;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsLoading);
    }

    public ObservableCollection<ChartPoint> RevenueByDay
    {
        get => _revenueByDay;
        private set => SetProperty(ref _revenueByDay, value);
    }

    public ObservableCollection<ChartPoint> ProfitByDay
    {
        get => _profitByDay;
        private set => SetProperty(ref _profitByDay, value);
    }

    public ObservableCollection<BarChartItem> RevenueByWeek
    {
        get => _revenueByWeek;
        private set => SetProperty(ref _revenueByWeek, value);
    }

    public ObservableCollection<BarChartItem> ProfitByWeek
    {
        get => _profitByWeek;
        private set => SetProperty(ref _profitByWeek, value);
    }

    public ObservableCollection<BarChartItem> RevenueByMonth
    {
        get => _revenueByMonth;
        private set => SetProperty(ref _revenueByMonth, value);
    }

    public ObservableCollection<BarChartItem> ProfitByMonth
    {
        get => _profitByMonth;
        private set => SetProperty(ref _profitByMonth, value);
    }

    public ObservableCollection<BarChartItem> RevenueByYear
    {
        get => _revenueByYear;
        private set => SetProperty(ref _revenueByYear, value);
    }

    public ObservableCollection<BarChartItem> ProfitByYear
    {
        get => _profitByYear;
        private set => SetProperty(ref _profitByYear, value);
    }

    public ObservableCollection<BarChartItem> ProductSalesByRange
    {
        get => _productSalesByRange;
        private set => SetProperty(ref _productSalesByRange, value);
    }

    public ObservableCollection<PieChartItem> ProductSalesShare
    {
        get => _productSalesShare;
        private set => SetProperty(ref _productSalesShare, value);
    }

    public ObservableCollection<SalesCommissionSnapshot> SalesCommissions
    {
        get => _salesCommissions;
        private set => SetProperty(ref _salesCommissions, value);
    }

    public ObservableCollection<MlInsight> MlInsights
    {
        get => _mlInsights;
        private set => SetProperty(ref _mlInsights, value);
    }

    public string AssistantSummary
    {
        get => _assistantSummary;
        private set => SetProperty(ref _assistantSummary, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public DateTimeOffset FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTimeOffset ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public string RangeLabel
    {
        get => _rangeLabel;
        set => SetProperty(ref _rangeLabel, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public decimal TotalRevenue
    {
        get => _totalRevenue;
        private set
        {
            if (SetProperty(ref _totalRevenue, value))
            {
                OnPropertyChanged(nameof(TotalRevenueText));
            }
        }
    }

    public decimal TotalProfit
    {
        get => _totalProfit;
        private set
        {
            if (SetProperty(ref _totalProfit, value))
            {
                OnPropertyChanged(nameof(TotalProfitText));
            }
        }
    }

    public string TotalRevenueText => CurrencyFormatter.ToCurrency(TotalRevenue);

    public string TotalProfitText => CurrencyFormatter.ToCurrency(TotalProfit);

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasLoadedSnapshot
    {
        get => _hasLoadedSnapshot;
        private set => SetProperty(ref _hasLoadedSnapshot, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        var fromDate = FromDate.Date;
        var toDate = ToDate.Date;
        if (HasLoadedSnapshot && _loadedFromDate == fromDate && _loadedToDate == toDate)
        {
            StatusMessage = "Showing cached report. Press Apply Range to refresh.";
            return;
        }

        await StartLoadAsync(force: false, cancellationToken);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await StartLoadAsync(force: true, cancellationToken);
    }

    public void CancelLoading()
    {
        _activeLoadCts?.Cancel();
    }

    private async Task StartLoadAsync(bool force, CancellationToken cancellationToken)
    {
        if (IsLoading && !force)
        {
            return;
        }

        _activeLoadCts?.Cancel();
        _activeLoadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var loadCts = _activeLoadCts;
        var token = loadCts.Token;
        var loadVersion = ++_loadVersion;
        var insightsStarted = false;

        IsLoading = true;
        StatusMessage = "Loading reports...";

        try
        {
            var fromDate = FromDate.Date;
            var toDate = ToDate.Date;
            var snapshot = await _reportingService.GetCoreSnapshotAsync(new ReportQueryOptions
            {
                FromDate = fromDate,
                ToDate = toDate
            }, token);

            token.ThrowIfCancellationRequested();
            if (!IsCurrentLoad(loadVersion, loadCts))
            {
                return;
            }

            ApplyCoreSnapshot(snapshot);
            _loadedFromDate = fromDate;
            _loadedToDate = toDate;
            HasLoadedSnapshot = true;
            StatusMessage = $"Core reports updated at {DateTime.Now:HH:mm:ss}. Loading insights...";
            insightsStarted = true;
            _ = LoadInsightsAsync(snapshot, loadVersion, loadCts);
        }
        catch (OperationCanceledException)
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                StatusMessage = "Reports loading was canceled.";
            }
        }
        catch (Exception ex)
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                StatusMessage = $"Reports could not be loaded: {ex.Message}";
            }
        }
        finally
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                IsLoading = false;
            }

            if (!insightsStarted)
            {
                if (IsCurrentLoad(loadVersion, loadCts))
                {
                    _activeLoadCts = null;
                }

                loadCts.Dispose();
            }
        }
    }

    public async Task RefreshAsync()
    {
        await StartLoadAsync(force: true, CancellationToken.None);
    }

    private async Task LoadInsightsAsync(ReportsSnapshot snapshot, int loadVersion, CancellationTokenSource loadCts)
    {
        try
        {
            var token = loadCts.Token;
            var insights = await _reportingService.GetReportInsightsAsync(snapshot, token);

            token.ThrowIfCancellationRequested();
            if (!IsCurrentLoad(loadVersion, loadCts))
            {
                return;
            }

            MlInsights = new ObservableCollection<MlInsight>(insights.MlInsights);
            AssistantSummary = insights.AssistantResult.Summary;
            StatusMessage = $"Reports and insights updated at {DateTime.Now:HH:mm:ss}.";
        }
        catch (OperationCanceledException)
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                StatusMessage = "Report insights loading was canceled.";
            }
        }
        catch (Exception ex)
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                StatusMessage = $"Core reports loaded. Insights could not be loaded: {ex.Message}";
            }
        }
        finally
        {
            if (IsCurrentLoad(loadVersion, loadCts))
            {
                _activeLoadCts = null;
            }

            loadCts.Dispose();
        }
    }

    private void ApplyCoreSnapshot(ReportsSnapshot snapshot)
    {
        RangeLabel = snapshot.RangeLabel;
        TotalRevenue = snapshot.TotalRevenue;
        TotalProfit = snapshot.TotalProfit;
        RevenueByDay = new ObservableCollection<ChartPoint>(snapshot.RevenueByDay);
        ProfitByDay = new ObservableCollection<ChartPoint>(snapshot.ProfitByDay);
        RevenueByWeek = new ObservableCollection<BarChartItem>(snapshot.RevenueByWeek);
        ProfitByWeek = new ObservableCollection<BarChartItem>(snapshot.ProfitByWeek);
        RevenueByMonth = new ObservableCollection<BarChartItem>(snapshot.RevenueByMonth);
        ProfitByMonth = new ObservableCollection<BarChartItem>(snapshot.ProfitByMonth);
        RevenueByYear = new ObservableCollection<BarChartItem>(snapshot.RevenueByYear);
        ProfitByYear = new ObservableCollection<BarChartItem>(snapshot.ProfitByYear);
        ProductSalesByRange = new ObservableCollection<BarChartItem>(snapshot.ProductSalesByRange);
        ProductSalesShare = new ObservableCollection<PieChartItem>(snapshot.ProductSalesShare);
        SalesCommissions = new ObservableCollection<SalesCommissionSnapshot>(snapshot.SalesCommissions);
        MlInsights = [];
        AssistantSummary = "Assistant summary is loading after the core report.";
    }

    private bool IsCurrentLoad(int loadVersion, CancellationTokenSource loadCts)
    {
        return loadVersion == _loadVersion && ReferenceEquals(loadCts, _activeLoadCts);
    }
}
