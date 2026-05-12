using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class ReportsViewModel : ViewModelBase
{
    private readonly ReportingService _reportingService;
    private bool _isLoading;
    private DateTimeOffset _fromDate = new(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
    private DateTimeOffset _toDate = new(DateTime.Today);
    private string _rangeLabel = string.Empty;
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
    private string _assistantSummary = string.Empty;

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

    public async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var snapshot = await _reportingService.GetSnapshotAsync(new ReportQueryOptions
            {
                FromDate = FromDate.Date,
                ToDate = ToDate.Date
            });

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
            MlInsights = new ObservableCollection<MlInsight>(snapshot.MlInsights);
            AssistantSummary = snapshot.AssistantResult.Summary;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }

        await LoadAsync();
    }
}
