using ProjectTest.Helpers;
using ProjectTest.Models;
using ProjectTest.Repositories;
using System.Collections.ObjectModel;

namespace ProjectTest.ViewModels;

public class CustomersViewModel : ViewModelBase
{
    private readonly CustomerRepository _customerRepository;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly AsyncRelayCommand _applySearchCommand;
    private readonly AsyncRelayCommand _saveCommand;
    private readonly AsyncRelayCommand _deleteCommand;
    private readonly RelayCommand _newCommand;
    private readonly RelayCommand _clearSearchCommand;
    private Customer? _selectedCustomer;
    private CustomerProfile? _selectedProfile;
    private int _editingCustomerId;
    private string _searchKeyword = string.Empty;
    private string _name = string.Empty;
    private string _phone = string.Empty;
    private string _email = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoading;

    public CustomersViewModel(CustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
        Customers = [];
        PurchasedProducts = [];
        OrderHistory = [];
        _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsLoading);
        _applySearchCommand = new AsyncRelayCommand(LoadAsync, () => !IsLoading);
        _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(Name));
        _deleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsLoading && SelectedCustomer is not null);
        _newCommand = new RelayCommand(NewCustomer, () => !IsLoading);
        _clearSearchCommand = new RelayCommand(ClearSearch, () => !IsLoading && !string.IsNullOrWhiteSpace(SearchKeyword));
    }

    public ObservableCollection<Customer> Customers { get; }

    public ObservableCollection<CustomerPurchasedProduct> PurchasedProducts { get; }

    public ObservableCollection<CustomerOrderHistory> OrderHistory { get; }

    public AsyncRelayCommand RefreshCommand => _refreshCommand;

    public AsyncRelayCommand ApplySearchCommand => _applySearchCommand;

    public AsyncRelayCommand SaveCommand => _saveCommand;

    public AsyncRelayCommand DeleteCommand => _deleteCommand;

    public RelayCommand NewCommand => _newCommand;

    public RelayCommand ClearSearchCommand => _clearSearchCommand;

    public string SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (SetProperty(ref _searchKeyword, value))
            {
                _clearSearchCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                _deleteCommand.NotifyCanExecuteChanged();
                if (value is not null)
                {
                    _ = LoadProfileAsync(value.Id);
                }
            }
        }
    }

    public CustomerProfile? SelectedProfile
    {
        get => _selectedProfile;
        private set
        {
            if (SetProperty(ref _selectedProfile, value))
            {
                OnPropertyChanged(nameof(ProfileSummary));
                OnPropertyChanged(nameof(LoyaltySummary));
                OnPropertyChanged(nameof(TotalSpendText));
                OnPropertyChanged(nameof(PaidOrdersText));
                OnPropertyChanged(nameof(LifetimeSpendText));
            }
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _saveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _refreshCommand.NotifyCanExecuteChanged();
                _applySearchCommand.NotifyCanExecuteChanged();
                _saveCommand.NotifyCanExecuteChanged();
                _deleteCommand.NotifyCanExecuteChanged();
                _newCommand.NotifyCanExecuteChanged();
                _clearSearchCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ProfileSummary => SelectedProfile is null
        ? "Select a customer to view order history."
        : $"{SelectedProfile.TotalOrderCount} order(s), {SelectedProfile.PaidOrderCount} paid.";

    public string LoyaltySummary => SelectedProfile?.Customer.LoyaltySummary ?? "0 points";

    public string TotalSpendText => SelectedProfile?.TotalSpendText ?? Helpers.CurrencyFormatter.ToCurrency(0m);

    public string PaidOrdersText => SelectedProfile is null ? "0" : SelectedProfile.PaidOrderCount.ToString();

    public string LifetimeSpendText => SelectedProfile?.Customer.LifetimeSpendText ?? Helpers.CurrencyFormatter.ToCurrency(0m);

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var selectedId = SelectedCustomer?.Id;
            Customers.Clear();
            foreach (var customer in await _customerRepository.SearchAsync(SearchKeyword))
            {
                Customers.Add(customer);
            }

            SelectedCustomer = Customers.FirstOrDefault(x => x.Id == selectedId) ?? Customers.FirstOrDefault();
            if (SelectedCustomer is null)
            {
                NewCustomer();
            }

            StatusMessage = Customers.Count == 0
                ? "No customers match this search."
                : string.IsNullOrWhiteSpace(SearchKeyword)
                    ? $"Loaded {Customers.Count} customer(s)."
                    : $"Found {Customers.Count} customer(s).";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProfileAsync(int customerId)
    {
        var profile = await _customerRepository.GetProfileAsync(customerId);
        if (profile is null)
        {
            StatusMessage = "Customer not found.";
            return;
        }

        SelectedProfile = profile;
        _editingCustomerId = profile.Customer.Id;
        Name = profile.Customer.Name;
        Phone = profile.Customer.Phone;
        Email = profile.Customer.Email;

        PurchasedProducts.Clear();
        foreach (var product in profile.PurchasedProducts)
        {
            PurchasedProducts.Add(product);
        }

        OrderHistory.Clear();
        foreach (var order in profile.Orders)
        {
            OrderHistory.Add(order);
        }
    }

    private void NewCustomer()
    {
        _editingCustomerId = 0;
        Name = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        SelectedProfile = null;
        PurchasedProducts.Clear();
        OrderHistory.Clear();
        StatusMessage = "Enter customer information, then save.";
    }

    private void ClearSearch()
    {
        SearchKeyword = string.Empty;
        _ = LoadAsync();
    }

    private async Task SaveAsync()
    {
        var result = await _customerRepository.SaveAsync(new Customer
        {
            Id = _editingCustomerId,
            Name = Name,
            Phone = Phone,
            Email = Email
        });

        StatusMessage = result.Message;
        if (!result.Success)
        {
            return;
        }

        await LoadAsync();
        SelectedCustomer = Customers.FirstOrDefault(x => x.Id == result.Value);
    }

    private async Task DeleteAsync()
    {
        if (SelectedCustomer is null)
        {
            StatusMessage = "Select a customer first.";
            return;
        }

        var result = await _customerRepository.DeleteAsync(SelectedCustomer.Id);
        StatusMessage = result.Message;
        if (result.Success)
        {
            await LoadAsync();
        }
    }
}
