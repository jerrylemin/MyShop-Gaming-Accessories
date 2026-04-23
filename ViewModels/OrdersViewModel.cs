using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class OrdersViewModel : ViewModelBase
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly SettingsService _settingsService;
    private readonly AsyncRelayCommand _saveCommand;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly AsyncRelayCommand _applyDateFilterCommand;
    private readonly RelayCommand _firstPageCommand;
    private readonly RelayCommand _previousPageCommand;
    private readonly RelayCommand _nextPageCommand;
    private readonly RelayCommand _lastPageCommand;
    private int _currentOrderId;
    private OrderStatus _persistedStatus = OrderStatus.Created;
    private OrderSummary? _selectedOrder;
    private ProductLookupItem? _selectedProductToAdd;
    private OrderStatusFilterOption? _selectedStatusFilter;
    private string _keyword = string.Empty;
    private OrderSortOption _selectedSortOption = OrderSortOption.LatestFirst;
    private DateTimeOffset _fromDate = new(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
    private DateTimeOffset _toDate = new(DateTime.Today);
    private DateTimeOffset _draftCreatedTime = DateTimeOffset.Now;
    private OrderStatus _selectedStatus = OrderStatus.Created;
    private decimal _draftTotal;
    private string _statusMessage = string.Empty;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalItems;
    private int _pageSize;

    public OrdersViewModel(OrderRepository orderRepository, ProductRepository productRepository, SettingsService settingsService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _settingsService = settingsService;

        Orders = new ObservableCollection<OrderSummary>();
        AvailableProducts = new ObservableCollection<ProductLookupItem>();
        DraftItems = new ObservableCollection<OrderLineViewModel>();
        StatusFilters = new ObservableCollection<OrderStatusFilterOption>
        {
            new() { Name = "All statuses" },
            new() { Name = "Created", Status = OrderStatus.Created },
            new() { Name = "Paid", Status = OrderStatus.Paid },
            new() { Name = "Cancelled", Status = OrderStatus.Cancelled }
        };
        SortOptions = new ObservableCollection<OrderSortOption>(Enum.GetValues<OrderSortOption>());

        _saveCommand = new AsyncRelayCommand(SaveAsync, () => CanEditCurrentOrder);
        _refreshCommand = new AsyncRelayCommand(() => LoadAsync(CurrentPage));
        _applyDateFilterCommand = new AsyncRelayCommand(() => LoadAsync(1));
        _firstPageCommand = new RelayCommand(() => _ = LoadAsync(1), () => CurrentPage > 1);
        _previousPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage - 1), () => CurrentPage > 1);
        _nextPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage + 1), () => CurrentPage < TotalPages);
        _lastPageCommand = new RelayCommand(() => _ = LoadAsync(TotalPages), () => CurrentPage < TotalPages);
        NewOrderCommand = new RelayCommand(NewOrder);
        AddItemCommand = new RelayCommand(AddSelectedProduct, () => SelectedProductToAdd is not null && CanEditCurrentOrder);

        _settingsService.SettingsChanged += (_, settings) =>
        {
            PageSize = settings.ItemsPerPage;
            _ = LoadAsync(1);
        };
    }

    public ObservableCollection<OrderSummary> Orders { get; }

    public ObservableCollection<ProductLookupItem> AvailableProducts { get; }

    public ObservableCollection<OrderLineViewModel> DraftItems { get; }

    public ObservableCollection<OrderStatus> StatusOptions { get; } = new(Enum.GetValues<OrderStatus>());

    public ObservableCollection<OrderStatusFilterOption> StatusFilters { get; }

    public ObservableCollection<OrderSortOption> SortOptions { get; }

    public AsyncRelayCommand SaveCommand => _saveCommand;

    public AsyncRelayCommand RefreshCommand => _refreshCommand;

    public AsyncRelayCommand ApplyDateFilterCommand => _applyDateFilterCommand;

    public RelayCommand FirstPageCommand => _firstPageCommand;

    public RelayCommand PreviousPageCommand => _previousPageCommand;

    public RelayCommand NextPageCommand => _nextPageCommand;

    public RelayCommand LastPageCommand => _lastPageCommand;

    public RelayCommand NewOrderCommand { get; }

    public RelayCommand AddItemCommand { get; }

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

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public OrderStatusFilterOption? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
    }

    public OrderSortOption SelectedSortOption
    {
        get => _selectedSortOption;
        set => SetProperty(ref _selectedSortOption, value);
    }

    public OrderSummary? SelectedOrder
    {
        get => _selectedOrder;
        set => SetProperty(ref _selectedOrder, value);
    }

    public ProductLookupItem? SelectedProductToAdd
    {
        get => _selectedProductToAdd;
        set
        {
            if (SetProperty(ref _selectedProductToAdd, value))
            {
                AddItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public DateTimeOffset DraftCreatedTime
    {
        get => _draftCreatedTime;
        set => SetProperty(ref _draftCreatedTime, value);
    }

    public OrderStatus SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                _saveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public decimal DraftTotal
    {
        get => _draftTotal;
        private set
        {
            if (SetProperty(ref _draftTotal, value))
            {
                OnPropertyChanged(nameof(DraftTotalText));
            }
        }
    }

    public string DraftTotalText => CurrencyFormatter.ToCurrency(DraftTotal);

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(PageSummary));
                UpdatePagingCommands();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        private set
        {
            if (SetProperty(ref _pageSize, value))
            {
                OnPropertyChanged(nameof(PageSummary));
            }
        }
    }

    public int TotalItems
    {
        get => _totalItems;
        private set
        {
            if (SetProperty(ref _totalItems, value))
            {
                OnPropertyChanged(nameof(PageSummary));
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (SetProperty(ref _totalPages, value))
            {
                OnPropertyChanged(nameof(PageSummary));
                UpdatePagingCommands();
            }
        }
    }

    public string PageSummary => $"Page {CurrentPage} of {TotalPages} | {TotalItems} orders | Page size {PageSize}";

    public bool CanEditCurrentOrder => CurrentOrderId == 0 || _persistedStatus == OrderStatus.Created;

    public async Task LoadAsync(int? pageNumber = null)
    {
        PageSize = Math.Max(1, _settingsService.CurrentSettings.ItemsPerPage);
        var currentSelectedOrderId = SelectedOrder?.Id ?? CurrentOrderId;
        var result = await _orderRepository.GetPagedAsync(new OrderQueryOptions
        {
            FromDate = FromDate.Date,
            ToDate = ToDate.Date,
            Keyword = Keyword.Trim(),
            Status = SelectedStatusFilter?.Status,
            SortOption = SelectedSortOption,
            PageNumber = Math.Max(1, pageNumber ?? CurrentPage),
            PageSize = PageSize
        });

        Orders.Clear();
        foreach (var order in result.Items)
        {
            Orders.Add(order);
        }

        CurrentPage = result.PageNumber;
        TotalPages = result.TotalPages;
        TotalItems = result.TotalCount;

        AvailableProducts.Clear();
        foreach (var product in await _productRepository.GetLookupAsync())
        {
            AvailableProducts.Add(product);
        }

        SelectedOrder = Orders.FirstOrDefault(x => x.Id == currentSelectedOrderId) ?? Orders.FirstOrDefault();

        if (SelectedOrder is not null && SelectedOrder.Id != CurrentOrderId)
        {
            await LoadOrderAsync(SelectedOrder.Id);
        }
        else if (CurrentOrderId == 0)
        {
            NewOrder();
        }
    }

    public async Task LoadOrderAsync(int orderId)
    {
        var draft = await _orderRepository.GetDraftByIdAsync(orderId);
        if (draft is null)
        {
            StatusMessage = "Order not found.";
            return;
        }

        CurrentOrderId = draft.Id;
        _persistedStatus = draft.Status;
        DraftCreatedTime = draft.CreatedTime;
        SelectedStatus = draft.Status;
        DraftItems.Clear();

        foreach (var item in draft.Items)
        {
            AddLine(item);
        }

        RecalculateTotals();
        RefreshEditorState();
    }

    public async Task DeleteSelectedAsync()
    {
        var orderId = SelectedOrder?.Id ?? CurrentOrderId;
        if (orderId == 0)
        {
            StatusMessage = "Select an order first.";
            return;
        }

        var result = await _orderRepository.DeleteAsync(orderId);
        StatusMessage = result.Message;
        if (result.Success)
        {
            CurrentOrderId = 0;
            _persistedStatus = OrderStatus.Created;
            await LoadAsync(CurrentPage);
            NewOrder();
        }
    }

    public void RemoveLine(OrderLineViewModel line)
    {
        line.LineChanged -= DraftLineChanged;
        DraftItems.Remove(line);
        RecalculateTotals();
    }

    private int CurrentOrderId
    {
        get => _currentOrderId;
        set
        {
            if (SetProperty(ref _currentOrderId, value))
            {
                RefreshEditorState();
            }
        }
    }

    private void NewOrder()
    {
        CurrentOrderId = 0;
        _persistedStatus = OrderStatus.Created;
        SelectedOrder = null;
        DraftCreatedTime = DateTimeOffset.Now;
        SelectedStatus = OrderStatus.Created;
        DraftItems.Clear();
        DraftTotal = 0m;
        StatusMessage = string.Empty;
        RefreshEditorState();
    }

    private void AddSelectedProduct()
    {
        if (SelectedProductToAdd is null)
        {
            return;
        }

        var existing = DraftItems.FirstOrDefault(x => x.ProductId == SelectedProductToAdd.Id);
        if (existing is not null)
        {
            existing.Quantity += 1;
            return;
        }

        AddLine(new OrderDraftItem
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            Manufacturer = SelectedProductToAdd.Manufacturer,
            UnitSalePrice = SelectedProductToAdd.SalePrice,
            Quantity = 1,
            AvailableStock = SelectedProductToAdd.Stock,
            ImagePath = SelectedProductToAdd.ImagePath
        });
    }

    private void AddLine(OrderDraftItem item)
    {
        var line = new OrderLineViewModel
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Manufacturer = item.Manufacturer,
            UnitSalePrice = item.UnitSalePrice,
            AvailableStock = item.AvailableStock,
            ImagePath = item.ImagePath,
            Quantity = item.Quantity
        };

        line.LineChanged += DraftLineChanged;
        DraftItems.Add(line);
        RecalculateTotals();
    }

    private async Task SaveAsync()
    {
        if (DraftItems.Count == 0)
        {
            StatusMessage = "Add at least one product.";
            return;
        }

        if (!CanEditCurrentOrder)
        {
            StatusMessage = "Paid and cancelled orders are read-only.";
            return;
        }

        var draft = new OrderDraft
        {
            Id = CurrentOrderId,
            CreatedTime = DraftCreatedTime.DateTime,
            Status = SelectedStatus,
            Items = DraftItems.Select(x => new OrderDraftItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Manufacturer = x.Manufacturer,
                UnitSalePrice = x.UnitSalePrice,
                Quantity = x.Quantity,
                AvailableStock = x.AvailableStock,
                ImagePath = x.ImagePath
            }).ToList()
        };

        var result = await _orderRepository.SaveAsync(draft);
        StatusMessage = result.Message;
        if (result.Success)
        {
            CurrentOrderId = result.Value;
            _persistedStatus = SelectedStatus;
            await LoadAsync(CurrentPage);
            if (CurrentOrderId != 0)
            {
                await LoadOrderAsync(CurrentOrderId);
            }
        }
    }

    private void DraftLineChanged(object? sender, EventArgs e)
    {
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        DraftTotal = DraftItems.Sum(x => x.LineTotal);
    }

    private void RefreshEditorState()
    {
        OnPropertyChanged(nameof(CanEditCurrentOrder));
        _saveCommand.NotifyCanExecuteChanged();
        AddItemCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePagingCommands()
    {
        _firstPageCommand.NotifyCanExecuteChanged();
        _previousPageCommand.NotifyCanExecuteChanged();
        _nextPageCommand.NotifyCanExecuteChanged();
        _lastPageCommand.NotifyCanExecuteChanged();
    }
}
