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
    private ObservableCollection<ChartPoint> _revenueByDay = [];
    private ObservableCollection<BarChartItem> _revenueByWeek = [];
    private ObservableCollection<BarChartItem> _revenueByMonth = [];
    private ObservableCollection<BarChartItem> _revenueByYear = [];
    private ObservableCollection<BarChartItem> _productSalesByRange = [];
    private ObservableCollection<PieChartItem> _productSalesShare = [];

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

    public ObservableCollection<BarChartItem> RevenueByWeek
    {
        get => _revenueByWeek;
        private set => SetProperty(ref _revenueByWeek, value);
    }

    public ObservableCollection<BarChartItem> RevenueByMonth
    {
        get => _revenueByMonth;
        private set => SetProperty(ref _revenueByMonth, value);
    }

    public ObservableCollection<BarChartItem> RevenueByYear
    {
        get => _revenueByYear;
        private set => SetProperty(ref _revenueByYear, value);
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
            RevenueByDay = new ObservableCollection<ChartPoint>(snapshot.RevenueByDay);
            RevenueByWeek = new ObservableCollection<BarChartItem>(snapshot.RevenueByWeek);
            RevenueByMonth = new ObservableCollection<BarChartItem>(snapshot.RevenueByMonth);
            RevenueByYear = new ObservableCollection<BarChartItem>(snapshot.RevenueByYear);
            ProductSalesByRange = new ObservableCollection<BarChartItem>(snapshot.ProductSalesByRange);
            ProductSalesShare = new ObservableCollection<PieChartItem>(snapshot.ProductSalesShare);
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
