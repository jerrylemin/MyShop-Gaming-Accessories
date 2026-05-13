using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace ProjectTest.ViewModels;

public class OrdersViewModel : ViewModelBase
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly SettingsService _settingsService;
    private readonly CustomerRepository _customerRepository;
    private readonly PromotionRepository _promotionRepository;
    private readonly CurrentUserService _currentUserService;
    private readonly InvoiceExportService _invoiceExportService;
    private readonly AsyncRelayCommand _saveCommand;
    private readonly AsyncRelayCommand _printCommand;
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
    private string _productSearchText = string.Empty;
    private Promotion? _selectedPromotion;
    private OrderStatusFilterOption? _selectedStatusFilter;
    private string _keyword = string.Empty;
    private string _customerPhone = string.Empty;
    private OrderSortOption _selectedSortOption = OrderSortOption.LatestFirst;
    private DateTimeOffset _fromDate = new(new DateTime(2000, 1, 1));
    private DateTimeOffset _toDate = new(new DateTime(2100, 12, 31));
    private DateTimeOffset _draftCreatedTime = DateTimeOffset.Now;
    private OrderStatus _selectedStatus = OrderStatus.Created;
    private decimal _draftTotal;
    private decimal _discountAmount;
    private string _statusMessage = string.Empty;
    private string _autoSaveStatus = "Manual save only";
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalItems;
    private int _pageSize;
    private bool _isLoadingDraft;

    public OrdersViewModel(
        OrderRepository orderRepository,
        ProductRepository productRepository,
        SettingsService settingsService,
        CustomerRepository? customerRepository = null,
        PromotionRepository? promotionRepository = null,
        CurrentUserService? currentUserService = null,
        InvoiceExportService? invoiceExportService = null)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _settingsService = settingsService;
        _customerRepository = customerRepository ?? App.Current.Services.CustomerRepository;
        _promotionRepository = promotionRepository ?? App.Current.Services.PromotionRepository;
        _currentUserService = currentUserService ?? App.Current.Services.CurrentUserService;
        _invoiceExportService = invoiceExportService ?? App.Current.Services.InvoiceExportService;

        Orders = [];
        AvailableProducts = [];
        FilteredAvailableProducts = [];
        Promotions = [];
        DraftItems = [];
        StatusFilters =
        [
            new() { Name = "All statuses" },
            new() { Name = "Created", Status = OrderStatus.Created },
            new() { Name = "Paid", Status = OrderStatus.Paid },
            new() { Name = "Cancelled", Status = OrderStatus.Cancelled }
        ];
        SortOptions = new ObservableCollection<OrderSortOption>(Enum.GetValues<OrderSortOption>());

        _saveCommand = new AsyncRelayCommand(SaveAsync, () => CanEditCurrentOrder);
        _printCommand = new AsyncRelayCommand(PrintAsync, () => CurrentOrderId != 0);
        _refreshCommand = new AsyncRelayCommand(() => LoadAsync(CurrentPage));
        _applyDateFilterCommand = new AsyncRelayCommand(() => LoadAsync(1));
        _firstPageCommand = new RelayCommand(() => _ = LoadAsync(1), () => CurrentPage > 1);
        _previousPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage - 1), () => CurrentPage > 1);
        _nextPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage + 1), () => CurrentPage < TotalPages);
        _lastPageCommand = new RelayCommand(() => _ = LoadAsync(TotalPages), () => CurrentPage < TotalPages);
        NewOrderCommand = new RelayCommand(NewOrder, () => CanEditOrders);
        AddItemCommand = new RelayCommand(AddSelectedProduct, () => SelectedProductToAdd is not null && CanEditCurrentOrder);


        _settingsService.SettingsChanged += (_, settings) =>
        {
            PageSize = settings.ItemsPerPage;
            _ = LoadAsync(1);
        };
    }

    public ObservableCollection<OrderSummary> Orders { get; }

    public ObservableCollection<ProductLookupItem> AvailableProducts { get; }

    public ObservableCollection<ProductLookupItem> FilteredAvailableProducts { get; }

    public ObservableCollection<Promotion> Promotions { get; }

    public ObservableCollection<OrderLineViewModel> DraftItems { get; }

    public event EventHandler<CreateCustomerRequestedEventArgs>? CreateCustomerRequested;

    public ObservableCollection<OrderStatus> StatusOptions { get; } = new(Enum.GetValues<OrderStatus>());

    public ObservableCollection<OrderStatusFilterOption> StatusFilters { get; }

    public ObservableCollection<OrderSortOption> SortOptions { get; }

    public AsyncRelayCommand SaveCommand => _saveCommand;

    public AsyncRelayCommand PrintCommand => _printCommand;

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

    public string ProductSearchText
    {
        get => _productSearchText;
        set
        {
            if (SetProperty(ref _productSearchText, value))
            {
                RefreshFilteredProducts();
            }
        }
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set => SetProperty(ref _customerPhone, value);
    }

    public Promotion? SelectedPromotion
    {
        get => _selectedPromotion;
        set
        {
            if (SetProperty(ref _selectedPromotion, value))
            {
                RecalculateTotals();
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

    public decimal DiscountAmount
    {
        get => _discountAmount;
        private set
        {
            if (SetProperty(ref _discountAmount, value))
            {
                OnPropertyChanged(nameof(DiscountAmountText));
            }
        }
    }

    public string DiscountAmountText => CurrencyFormatter.ToCurrency(DiscountAmount);

    public string EditorActionText => CurrentOrderId == 0 ? "Create Order" : "Update Order";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string AutoSaveStatus
    {
        get => _autoSaveStatus;
        set => SetProperty(ref _autoSaveStatus, value);
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

    public bool CanEditOrders => _currentUserService.CanManageOrders;

    public Visibility OrderEditingVisibility => CanEditOrders ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OrderReadOnlyVisibility => CanEditOrders ? Visibility.Collapsed : Visibility.Visible;

    public string RolePermissionSummary => _currentUserService.RoleDisplayName;

    public bool CanEditCurrentOrder => CanEditOrders && (CurrentOrderId == 0 || _persistedStatus == OrderStatus.Created);

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
            CurrentUserId = _currentUserService.CurrentUser.Id,
            CurrentUserRole = _currentUserService.CurrentUser.Role,
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

        RefreshFilteredProducts();

        Promotions.Clear();
        foreach (var promotion in await _promotionRepository.GetActiveAsync())
        {
            Promotions.Add(promotion);
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

        _isLoadingDraft = true;
        try
        {
            CurrentOrderId = draft.Id;
            _persistedStatus = draft.Status;
            DraftCreatedTime = draft.CreatedTime;
            SelectedStatus = draft.Status;
            CustomerPhone = draft.CustomerId.HasValue
                ? (await _customerRepository.GetByIdAsync(draft.CustomerId.Value))?.Phone ?? string.Empty
                : string.Empty;
            SelectedPromotion = Promotions.FirstOrDefault(x => x.Id == draft.PromotionId);
            DiscountAmount = draft.DiscountAmount;
            DraftItems.Clear();

            foreach (var item in draft.Items)
            {
                AddLine(item);
            }

            RecalculateTotals();
            RefreshEditorState();
        }
        finally
        {
            _isLoadingDraft = false;
        }
    }

    public async Task DeleteSelectedAsync()
    {
        if (!CanEditOrders)
        {
            StatusMessage = "This role can review orders but cannot delete them.";
            return;
        }

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
        if (!CanEditOrders)
        {
            StatusMessage = "This role can review orders but cannot change order lines.";
            return;
        }

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
                OnPropertyChanged(nameof(EditorActionText));
                _printCommand.NotifyCanExecuteChanged();
                NewOrderCommand.NotifyCanExecuteChanged();
                RefreshEditorState();
            }
        }
    }

    private void NewOrder()
    {
        if (!CanEditOrders)
        {
            StatusMessage = "This role can review orders but cannot create new orders.";
            return;
        }

        _isLoadingDraft = true;
        CurrentOrderId = 0;
        _persistedStatus = OrderStatus.Created;
        SelectedOrder = null;
        DraftCreatedTime = DateTimeOffset.Now;
        SelectedStatus = OrderStatus.Created;
        CustomerPhone = string.Empty;
        SelectedPromotion = null;
        ProductSearchText = string.Empty;
        SelectedProductToAdd = null;
        DraftItems.Clear();
        DiscountAmount = 0m;
        DraftTotal = 0m;
        StatusMessage = string.Empty;
        AutoSaveStatus = "Manual save only";
        _isLoadingDraft = false;
        RefreshEditorState();
    }

    private void AddSelectedProduct()
    {
        if (!CanEditOrders)
        {
            StatusMessage = "This role can review orders but cannot add products to orders.";
            return;
        }

        if (SelectedProductToAdd is null)
        {
            return;
        }

        var existing = DraftItems.FirstOrDefault(x => x.ProductId == SelectedProductToAdd.Id);
        if (existing is not null)
        {
            existing.Quantity += 1;
            RecalculateTotals();
            ProductSearchText = string.Empty;
            SelectedProductToAdd = null;
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

        ProductSearchText = string.Empty;
        SelectedProductToAdd = null;
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
        await SaveAsync(allowCreateCustomerPrompt: true);
    }

    private async Task SaveAsync(bool allowCreateCustomerPrompt)
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

        StatusMessage = "Saving order...";
        AutoSaveStatus = "Saving...";

        StatusMessage = "Saving order...";
        AutoSaveStatus = "Saving...";

        var customer = await ResolveCustomerByPhoneAsync();
        if (!string.IsNullOrWhiteSpace(CustomerPhone) && customer is null)
        {
            if (!allowCreateCustomerPrompt)
            {
                StatusMessage = "Customer phone is new. Click Create Order to add customer info before saving.";
                return;
            }

            customer = await RequestNewCustomerAsync(CustomerPhone);
            if (customer is null)
            {
                StatusMessage = "Order was not saved because the customer was not created.";
                return;
            }
        }

        var isNewOrder = CurrentOrderId == 0;
        var draft = new OrderDraft
        {
            Id = CurrentOrderId,
            CreatedTime = DraftCreatedTime.DateTime,
            Status = SelectedStatus,
            CustomerId = customer?.Id,
            PromotionId = SelectedPromotion?.Id,
            CreatedByUserId = _currentUserService.CurrentUser.Id,
            DiscountAmount = DiscountAmount,
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
        if (!result.Success)
        {
            AutoSaveStatus = "Save failed";
            return;
        }

        CurrentOrderId = result.Value;
        _persistedStatus = SelectedStatus;
        AutoSaveStatus = "Saved";
        await LoadAsync(isNewOrder ? 1 : CurrentPage);

        if (CurrentOrderId != 0)
        {
            await LoadOrderAsync(CurrentOrderId);
        }
    }

    private void RefreshFilteredProducts()
    {
        if (FilteredAvailableProducts is null)
        {
            return;
        }

        var selectedProductId = SelectedProductToAdd?.Id;
        var search = ProductSearchText?.Trim() ?? string.Empty;
        var terms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IEnumerable<ProductLookupItem> query = AvailableProducts;

        if (terms.Length > 0)
        {
            query = query.Where(product =>
            {
                var searchText = product.SearchText;
                return terms.All(term => searchText.Contains(term, StringComparison.OrdinalIgnoreCase));
            });
        }

        var filteredProducts = query
            .OrderByDescending(product => product.Stock > 0)
            .ThenBy(product => product.Name)
            .Take(80)
            .ToList();

        FilteredAvailableProducts.Clear();
        foreach (var product in filteredProducts)
        {
            FilteredAvailableProducts.Add(product);
        }

        if (selectedProductId.HasValue)
        {
            SelectedProductToAdd = FilteredAvailableProducts.FirstOrDefault(product => product.Id == selectedProductId.Value);
        }
        else if (FilteredAvailableProducts.Count == 1)
        {
            SelectedProductToAdd = FilteredAvailableProducts[0];
        }
        else if (SelectedProductToAdd is not null &&
                 FilteredAvailableProducts.All(product => product.Id != SelectedProductToAdd.Id))
        {
            SelectedProductToAdd = null;
        }
    }
    private void DraftLineChanged(object? sender, EventArgs e)
    {
        RecalculateTotals();
    }

    private async Task<Customer?> ResolveCustomerByPhoneAsync()
    {
        return string.IsNullOrWhiteSpace(CustomerPhone)
            ? null
            : await _customerRepository.GetByPhoneAsync(CustomerPhone);
    }

    private async Task<Customer?> RequestNewCustomerAsync(string phone)
    {
        var args = new CreateCustomerRequestedEventArgs(phone.Trim());
        CreateCustomerRequested?.Invoke(this, args);
        return await args.WaitForCustomerAsync();
    }

    private void RecalculateTotals()
    {
        var subtotal = DraftItems.Sum(x => x.LineTotal);
        DiscountAmount = App.Current.Services.DiscountService.CalculateDiscount(subtotal, SelectedPromotion);
        DraftTotal = Math.Max(0m, subtotal - DiscountAmount);
    }


    private async Task PrintAsync()
    {
        if (CurrentOrderId == 0)
        {
            StatusMessage = "Save the order before exporting an invoice.";
            return;
        }

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = $"MyShop-Invoice-{CurrentOrderId}"
        };
        picker.FileTypeChoices.Add("PDF invoice", [".pdf"]);

        if (App.Current.ActiveWindow is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.Current.ActiveWindow));
        }

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            StatusMessage = "Invoice export cancelled.";
            return;
        }

        var result = await _invoiceExportService.ExportInvoiceAsync(CurrentOrderId, file.Path);
        StatusMessage = result.Success ? $"Invoice exported: {result.Value}" : result.Message;
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

public sealed class CreateCustomerRequestedEventArgs : EventArgs
{
    private readonly TaskCompletionSource<Customer?> _completion = new();

    public CreateCustomerRequestedEventArgs(string phone)
    {
        Phone = phone;
    }

    public string Phone { get; }

    public void SetResult(Customer? customer)
    {
        _completion.TrySetResult(customer);
    }

    public Task<Customer?> WaitForCustomerAsync()
    {
        return _completion.Task;
    }
}






















