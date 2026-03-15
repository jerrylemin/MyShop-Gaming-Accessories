using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly DashboardService _dashboardService;
    private bool _isLoading;
    private int _totalProducts;
    private int _lowStockProducts;
    private int _todayOrderCount;
    private decimal _todayRevenue;
    private ObservableCollection<ChartPoint> _revenuePoints = [];
    private ObservableCollection<Product> _topLowStockProducts = [];
    private ObservableCollection<ProductSalesSummary> _topSellingProducts = [];
    private ObservableCollection<OrderSummary> _latestOrders = [];

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsLoading);
    }

    public ObservableCollection<ChartPoint> RevenuePoints
    {
        get => _revenuePoints;
        private set => SetProperty(ref _revenuePoints, value);
    }

    public ObservableCollection<Product> TopLowStockProducts
    {
        get => _topLowStockProducts;
        private set => SetProperty(ref _topLowStockProducts, value);
    }

    public ObservableCollection<ProductSalesSummary> TopSellingProducts
    {
        get => _topSellingProducts;
        private set => SetProperty(ref _topSellingProducts, value);
    }

    public ObservableCollection<OrderSummary> LatestOrders
    {
        get => _latestOrders;
        private set => SetProperty(ref _latestOrders, value);
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public int TotalProducts
    {
        get => _totalProducts;
        private set
        {
            if (SetProperty(ref _totalProducts, value))
            {
                OnPropertyChanged(nameof(TotalProductsText));
            }
        }
    }

    public int LowStockProducts
    {
        get => _lowStockProducts;
        private set
        {
            if (SetProperty(ref _lowStockProducts, value))
            {
                OnPropertyChanged(nameof(LowStockProductsText));
            }
        }
    }

    public decimal TodayRevenue
    {
        get => _todayRevenue;
        private set
        {
            if (SetProperty(ref _todayRevenue, value))
            {
                OnPropertyChanged(nameof(TodayRevenueText));
            }
        }
    }

    public int TodayOrderCount
    {
        get => _todayOrderCount;
        private set
        {
            if (SetProperty(ref _todayOrderCount, value))
            {
                OnPropertyChanged(nameof(TodayOrderCountText));
            }
        }
    }

    public string TodayRevenueText => CurrencyFormatter.ToCurrency(TodayRevenue);

    public string TotalProductsText => TotalProducts.ToString();

    public string LowStockProductsText => LowStockProducts.ToString();

    public string TodayOrderCountText => TodayOrderCount.ToString();

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
            var snapshot = await _dashboardService.GetSnapshotAsync();
            TotalProducts = snapshot.TotalProducts;
            LowStockProducts = snapshot.LowStockProducts;
            TodayOrderCount = snapshot.TodayOrderCount;
            TodayRevenue = snapshot.TodayRevenue;

            RevenuePoints = new ObservableCollection<ChartPoint>(snapshot.RevenuePoints);
            TopLowStockProducts = new ObservableCollection<Product>(snapshot.TopLowStockProducts);
            TopSellingProducts = new ObservableCollection<ProductSalesSummary>(snapshot.TopSellingProducts);
            LatestOrders = new ObservableCollection<OrderSummary>(snapshot.LatestOrders);
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
