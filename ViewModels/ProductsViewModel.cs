using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class ProductsViewModel : ViewModelBase
{
    private readonly ProductRepository _productRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly ExcelProductImportService _excelProductImportService;
    private readonly SettingsService _settingsService;
    private readonly CurrentUserService _currentUserService;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly RelayCommand _previousPageCommand;
    private readonly RelayCommand _nextPageCommand;
    private readonly RelayCommand _addCommand;
    private readonly RelayCommand _editCommand;
    private readonly RelayCommand _addCategoryCommand;
    private readonly RelayCommand _editCategoryCommand;
    private readonly AsyncRelayCommand _deleteCategoryCommand;
    private Product? _selectedProduct;
    private CategoryFilterOption? _selectedCategoryFilter;
    private CategoryListItem? _selectedCategoryItem;
    private string _keyword = string.Empty;
    private string _minPrice = string.Empty;
    private string _maxPrice = string.Empty;
    private string _statusMessage = string.Empty;
    private ProductSortOption _selectedSortOption = ProductSortOption.Name;
    private bool _isLoading;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount;

    public ProductsViewModel(
        ProductRepository productRepository,
        CategoryRepository categoryRepository,
        ExcelProductImportService excelProductImportService,
        SettingsService settingsService,
        CurrentUserService? currentUserService = null)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _excelProductImportService = excelProductImportService;
        _settingsService = settingsService;
        _currentUserService = currentUserService ?? App.Current.Services.CurrentUserService;

        SortOptions = new ObservableCollection<ProductSortOption>(Enum.GetValues<ProductSortOption>());
        CategoryFilters = [];
        ManagedCategories = [];
        Products = [];

        _refreshCommand = new AsyncRelayCommand(() => LoadAsync(1), () => !IsLoading && CanViewProductList);
        _previousPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage - 1), () => CurrentPage > 1 && !IsLoading && CanViewProductList);
        _nextPageCommand = new RelayCommand(() => _ = LoadAsync(CurrentPage + 1), () => CurrentPage < TotalPages && !IsLoading && CanViewProductList);
        _addCommand = new RelayCommand(() => EditRequested?.Invoke(this, null), () => CanManageProducts);
        _editCommand = new RelayCommand(() => EditRequested?.Invoke(this, SelectedProduct?.Id), () => SelectedProduct is not null && CanManageProducts);
        _addCategoryCommand = new RelayCommand(() => CategoryEditRequested?.Invoke(this, null), () => CanManageCategories);
        _editCategoryCommand = new RelayCommand(() =>
        {
            if (SelectedCategoryItem is not null)
            {
                CategoryEditRequested?.Invoke(this, SelectedCategoryItem);
            }
        }, () => SelectedCategoryItem is not null && CanManageCategories);
        _deleteCategoryCommand = new AsyncRelayCommand(DeleteSelectedCategoryAsync, () => SelectedCategoryItem is not null && !IsLoading && CanManageCategories);

        _settingsService.SettingsChanged += (_, _) => _ = LoadAsync(1);
    }

    public event EventHandler<int?>? EditRequested;

    public event EventHandler<CategoryListItem?>? CategoryEditRequested;

    public ObservableCollection<Product> Products { get; }

    public bool CanViewProductList => _currentUserService.CanViewProductList;

    public bool CanViewImportPrice => _currentUserService.CanViewImportPrice;

    public bool CanManageProducts => _currentUserService.CanManageProducts;

    public bool CanImportProducts => _currentUserService.CanImportProducts;

    public bool CanManageCategories => _currentUserService.CanManageCategories;

    public Visibility ProductListVisibility => CanViewProductList ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ImportPriceVisibility => CanViewImportPrice ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProductManagementVisibility => CanManageProducts ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProductImportVisibility => CanImportProducts ? Visibility.Visible : Visibility.Collapsed;

    public Visibility CategoryManagementVisibility => CanManageCategories ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProductReadOnlyHintVisibility => CanManageProducts ? Visibility.Collapsed : Visibility.Visible;

    public string RolePermissionSummary => _currentUserService.RoleDisplayName;

    public ObservableCollection<ProductSortOption> SortOptions { get; }

    public ObservableCollection<CategoryFilterOption> CategoryFilters { get; }

    public ObservableCollection<CategoryListItem> ManagedCategories { get; }

    public AsyncRelayCommand RefreshCommand => _refreshCommand;

    public RelayCommand PreviousPageCommand => _previousPageCommand;

    public RelayCommand NextPageCommand => _nextPageCommand;

    public RelayCommand AddCommand => _addCommand;

    public RelayCommand EditCommand => _editCommand;

    public RelayCommand AddCategoryCommand => _addCategoryCommand;

    public RelayCommand EditCategoryCommand => _editCategoryCommand;

    public AsyncRelayCommand DeleteCategoryCommand => _deleteCategoryCommand;

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                _editCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public CategoryFilterOption? SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set => SetProperty(ref _selectedCategoryFilter, value);
    }

    public CategoryListItem? SelectedCategoryItem
    {
        get => _selectedCategoryItem;
        set
        {
            if (SetProperty(ref _selectedCategoryItem, value))
            {
                _editCategoryCommand.NotifyCanExecuteChanged();
                _deleteCategoryCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public string MinPrice
    {
        get => _minPrice;
        set => SetProperty(ref _minPrice, value);
    }

    public string MaxPrice
    {
        get => _maxPrice;
        set => SetProperty(ref _maxPrice, value);
    }

    public ProductSortOption SelectedSortOption
    {
        get => _selectedSortOption;
        set => SetProperty(ref _selectedSortOption, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _refreshCommand.NotifyCanExecuteChanged();
                _previousPageCommand.NotifyCanExecuteChanged();
                _nextPageCommand.NotifyCanExecuteChanged();
                _addCommand.NotifyCanExecuteChanged();
                _editCommand.NotifyCanExecuteChanged();
                _addCategoryCommand.NotifyCanExecuteChanged();
                _editCategoryCommand.NotifyCanExecuteChanged();
                _deleteCategoryCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(PageSummary));
                _previousPageCommand.NotifyCanExecuteChanged();
                _nextPageCommand.NotifyCanExecuteChanged();
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
                _previousPageCommand.NotifyCanExecuteChanged();
                _nextPageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(PageSummary));
            }
        }
    }

    public string PageSummary => $"Page {CurrentPage} of {TotalPages} ({TotalCount} items)";

    public async Task LoadAsync(int? pageNumber = null)
    {
        if (!CanViewProductList)
        {
            Products.Clear();
            StatusMessage = "This role cannot view products.";
            return;
        }

        IsLoading = true;

        try
        {
            await EnsureCategoryDataLoadedAsync();
            var result = await _productRepository.GetPagedAsync(new ProductQueryOptions
            {
                PageNumber = Math.Max(1, pageNumber ?? CurrentPage),
                PageSize = Math.Max(1, _settingsService.CurrentSettings.ItemsPerPage),
                Keyword = Keyword.Trim(),
                MinPrice = TryParseDecimal(MinPrice),
                MaxPrice = TryParseDecimal(MaxPrice),
                CategoryId = SelectedCategoryFilter?.Id,
                SortOption = SelectedSortOption,
                CurrentUserRole = _currentUserService.CurrentUser.Role
            });

            CurrentPage = result.PageNumber;
            TotalPages = result.TotalPages;
            TotalCount = result.TotalCount;

            Products.Clear();
            foreach (var item in result.Items)
            {
                Products.Add(item);
            }

            SelectedProduct = Products.FirstOrDefault();

            StatusMessage = Products.Count == 0
                ? "No products matched the current filters."
                : $"Loaded {Products.Count} product(s). {RolePermissionSummary}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ImportFromExcelAsync(string filePath)
    {
        if (!CanImportProducts)
        {
            StatusMessage = "Only Admin can import products.";
            return;
        }

        var result = await _excelProductImportService.ImportAsync(filePath);
        StatusMessage = result.Message;
        if (result.Success)
        {
            await EnsureCategoryDataLoadedAsync(forceReload: true);
            await LoadAsync(1);
        }
    }

    public async Task DeleteSelectedAsync()
    {
        if (!CanManageProducts)
        {
            StatusMessage = "Only Admin can delete products.";
            return;
        }

        if (SelectedProduct is null)
        {
            StatusMessage = "Select a product first.";
            return;
        }

        var result = await _productRepository.DeleteAsync(SelectedProduct.Id);
        StatusMessage = result.Message;
        if (result.Success)
        {
            await LoadAsync(CurrentPage);
        }
    }

    public async Task SaveCategoryAsync(int categoryId, string name, string description)
    {
        if (!CanManageCategories)
        {
            StatusMessage = "Only Admin can manage categories.";
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Category name is required.";
            return;
        }

        var result = await _categoryRepository.SaveAsync(new Category
        {
            Id = categoryId,
            Name = name.Trim(),
            Description = description.Trim()
        });

        StatusMessage = result.Message;
        if (!result.Success)
        {
            return;
        }

        await EnsureCategoryDataLoadedAsync(forceReload: true);
        await LoadAsync(1);
        SelectedCategoryItem = ManagedCategories.FirstOrDefault(x => x.Id == result.Value);
    }

    private static decimal? TryParseDecimal(string value)
    {
        return decimal.TryParse(value, out var parsed) ? parsed : null;
    }

    private async Task EnsureCategoryDataLoadedAsync(bool forceReload = false)
    {
        if (!forceReload && CategoryFilters.Count > 0 && ManagedCategories.Count > 0)
        {
            return;
        }

        var currentCategoryId = SelectedCategoryFilter?.Id;
        var currentManagedCategoryId = SelectedCategoryItem?.Id;
        CategoryFilters.Clear();
        ManagedCategories.Clear();
        CategoryFilters.Add(new CategoryFilterOption { Name = "All categories" });

        var categories = await _categoryRepository.GetAllAsync();
        foreach (var category in categories)
        {
            CategoryFilters.Add(new CategoryFilterOption
            {
                Id = category.Id,
                Name = category.Name
            });
        }

        foreach (var item in await _categoryRepository.GetListItemsAsync())
        {
            ManagedCategories.Add(item);
        }

        SelectedCategoryFilter = CategoryFilters.FirstOrDefault(x => x.Id == currentCategoryId) ?? CategoryFilters.FirstOrDefault();
        SelectedCategoryItem = ManagedCategories.FirstOrDefault(x => x.Id == currentManagedCategoryId) ?? ManagedCategories.FirstOrDefault();
    }

    private async Task DeleteSelectedCategoryAsync()
    {
        if (!CanManageCategories)
        {
            StatusMessage = "Only Admin can delete categories.";
            return;
        }

        if (SelectedCategoryItem is null)
        {
            StatusMessage = "Select a category first.";
            return;
        }

        var result = await _categoryRepository.DeleteAsync(SelectedCategoryItem.Id);
        StatusMessage = result.Message;
        if (!result.Success)
        {
            return;
        }

        await EnsureCategoryDataLoadedAsync(forceReload: true);
        await LoadAsync(1);
    }
}
