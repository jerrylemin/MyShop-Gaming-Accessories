using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using ProjectTest.Services;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class ProductEditViewModel : ViewModelBase
{
    private readonly ProductRepository _productRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly AutoSaveService _autoSaveService = new();
    private readonly AsyncRelayCommand _saveCommand;
    private int _productId;
    private string _sku = string.Empty;
    private string _name = string.Empty;
    private string _manufacturer = string.Empty;
    private string _cpu = string.Empty;
    private string _ram = string.Empty;
    private string _storage = string.Empty;
    private string _gpu = string.Empty;
    private string _screen = string.Empty;
    private string _importPrice = string.Empty;
    private string _salePrice = string.Empty;
    private string _stock = string.Empty;
    private int _selectedCategoryId;
    private string _description = string.Empty;
    private string _image1 = string.Empty;
    private string _image2 = string.Empty;
    private string _image3 = string.Empty;
    private string _statusMessage = string.Empty;
    private string _autoSaveStatus = "Saved";
    private bool _isLoading;

    public ProductEditViewModel(ProductRepository productRepository, CategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        Categories = new ObservableCollection<Category>();
        _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsLoading);
        _autoSaveService.StateChanged += (_, state) => AutoSaveStatus = state switch
        {
            AutoSaveState.Saving => "Saving...",
            AutoSaveState.Saved => "Saved",
            AutoSaveState.Error => "Error",
            _ => AutoSaveStatus
        };
    }

    public event EventHandler? Saved;

    public ObservableCollection<Category> Categories { get; }

    public AsyncRelayCommand SaveCommand => _saveCommand;

    public int ProductId
    {
        get => _productId;
        private set
        {
            if (SetProperty(ref _productId, value))
            {
                OnPropertyChanged(nameof(PageTitle));
            }
        }
    }

    public string PageTitle => ProductId == 0 ? "Add Product" : "Edit Product";

    public string SKU
    {
        get => _sku;
        set => SetAndScheduleAutoSave(ref _sku, value);
    }

    public string Name
    {
        get => _name;
        set => SetAndScheduleAutoSave(ref _name, value);
    }

    public string Manufacturer
    {
        get => _manufacturer;
        set => SetAndScheduleAutoSave(ref _manufacturer, value);
    }

    public string CPU
    {
        get => _cpu;
        set => SetAndScheduleAutoSave(ref _cpu, value);
    }

    public string RAM
    {
        get => _ram;
        set => SetAndScheduleAutoSave(ref _ram, value);
    }

    public string Storage
    {
        get => _storage;
        set => SetAndScheduleAutoSave(ref _storage, value);
    }

    public string GPU
    {
        get => _gpu;
        set => SetAndScheduleAutoSave(ref _gpu, value);
    }

    public string Screen
    {
        get => _screen;
        set => SetAndScheduleAutoSave(ref _screen, value);
    }

    public string ImportPrice
    {
        get => _importPrice;
        set => SetAndScheduleAutoSave(ref _importPrice, value);
    }

    public string SalePrice
    {
        get => _salePrice;
        set => SetAndScheduleAutoSave(ref _salePrice, value);
    }

    public string Stock
    {
        get => _stock;
        set => SetAndScheduleAutoSave(ref _stock, value);
    }

    public int SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetAndScheduleAutoSave(ref _selectedCategoryId, value);
    }

    public string Description
    {
        get => _description;
        set => SetAndScheduleAutoSave(ref _description, value);
    }

    public string Image1
    {
        get => _image1;
        set => SetAndScheduleAutoSave(ref _image1, value);
    }

    public string Image2
    {
        get => _image2;
        set => SetAndScheduleAutoSave(ref _image2, value);
    }

    public string Image3
    {
        get => _image3;
        set => SetAndScheduleAutoSave(ref _image3, value);
    }

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

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _saveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public async Task LoadAsync(int productId)
    {
        IsLoading = true;

        try
        {
            Categories.Clear();
            var categories = await _categoryRepository.GetAllAsync();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            if (productId == 0)
            {
                ResetForNewProduct();
                return;
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product is null)
            {
                StatusMessage = "Product not found.";
                ResetForNewProduct();
                return;
            }

            ProductId = product.Id;
            SKU = product.SKU;
            Name = product.Name;
            Manufacturer = product.Manufacturer;
            CPU = product.CPU;
            RAM = product.RAM;
            Storage = product.Storage;
            GPU = product.GPU;
            Screen = product.Screen;
            ImportPrice = product.ImportPrice.ToString("F2");
            SalePrice = product.SalePrice.ToString("F2");
            Stock = product.Stock.ToString();
            SelectedCategoryId = product.CategoryId;
            Description = product.Description;
            Image1 = product.Image1;
            Image2 = product.Image2;
            Image3 = product.Image3;
            StatusMessage = string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveAsync()
    {
        await SaveAsync(navigateAfterSave: true);
    }

    private async Task SaveAsync(bool navigateAfterSave)
    {
        if (!decimal.TryParse(ImportPrice, out var importPrice) ||
            !decimal.TryParse(SalePrice, out var salePrice) ||
            !int.TryParse(Stock, out var stockValue))
        {
            StatusMessage = "Enter valid numeric values for prices and stock.";
            return;
        }

        if (SelectedCategoryId == 0 || string.IsNullOrWhiteSpace(SKU) || string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "SKU, name, and category are required.";
            return;
        }

        var result = await _productRepository.SaveAsync(new Product
        {
            Id = ProductId,
            SKU = SKU.Trim(),
            Name = Name.Trim(),
            Manufacturer = Manufacturer.Trim(),
            CPU = CPU.Trim(),
            RAM = RAM.Trim(),
            Storage = Storage.Trim(),
            GPU = GPU.Trim(),
            Screen = Screen.Trim(),
            ImportPrice = importPrice,
            SalePrice = salePrice,
            Stock = stockValue,
            CategoryId = SelectedCategoryId,
            Description = Description.Trim(),
            Image1 = Image1.Trim(),
            Image2 = Image2.Trim(),
            Image3 = Image3.Trim()
        });

        StatusMessage = result.Message;
        if (!result.Success)
        {
            return;
        }

        ProductId = result.Value;
        if (navigateAfterSave)
        {
            Saved?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ScheduleAutoSave()
    {
        if (IsLoading || !CanAttemptAutoSave())
        {
            return;
        }

        AutoSaveStatus = "Saving...";
        _autoSaveService.Schedule(async _ => await SaveAsync(navigateAfterSave: false));
    }

    private bool CanAttemptAutoSave()
    {
        return SelectedCategoryId != 0 &&
               !string.IsNullOrWhiteSpace(SKU) &&
               !string.IsNullOrWhiteSpace(Name) &&
               decimal.TryParse(ImportPrice, out _) &&
               decimal.TryParse(SalePrice, out _) &&
               int.TryParse(Stock, out _);
    }

    private void SetAndScheduleAutoSave<T>(ref T storage, T value)
    {
        if (SetProperty(ref storage, value))
        {
            ScheduleAutoSave();
        }
    }

    private void ResetForNewProduct()
    {
        ProductId = 0;
        SKU = string.Empty;
        Name = string.Empty;
        Manufacturer = string.Empty;
        CPU = string.Empty;
        RAM = string.Empty;
        Storage = string.Empty;
        GPU = string.Empty;
        Screen = string.Empty;
        ImportPrice = string.Empty;
        SalePrice = string.Empty;
        Stock = "0";
        SelectedCategoryId = Categories.FirstOrDefault()?.Id ?? 0;
        Description = string.Empty;
        Image1 = string.Empty;
        Image2 = string.Empty;
        Image3 = string.Empty;
        StatusMessage = string.Empty;
    }
}
