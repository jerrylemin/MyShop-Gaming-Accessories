using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ProjectTest.Services;
using ProjectTest.Views.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProjectTest.Views;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly NavigationService _navigationService;
    private readonly Dictionary<string, Button> _navigationButtons;
    private string _currentPageTitle = "Dashboard";
    private string _currentNavigationSelection = "Dashboard";
    private bool _isNavigationReady;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            WriteInitializeComponentExceptionLog(ex);
            throw;
        }

        _navigationService = App.Current.Services.NavigationService;
        _navigationButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase)
        {
            ["Dashboard"] = DashboardButton,
            ["Products"] = ProductsButton,
            ["Orders"] = OrdersButton,
            ["Customers"] = CustomersButton,
            ["Reports"] = ReportsButton,
            ["GraphQL"] = GraphQlButton,
            ["Plugins"] = PluginsButton,
            ["Settings"] = SettingsButton
        };

        HeaderDateTextBlock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy");
        PageTitleTextBlock.Text = CurrentPageTitle;
        PageSubtitleTextBlock.Text = GetPageSubtitle(CurrentPageTitle);

        RegisterNavigationHoverStates();
        ContentFrame.Navigated += ContentFrame_Navigated;
        ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;
        ContentFrame.Loaded += ContentFrame_Loaded;
        Activated += MainWindow_Activated;
        SizeChanged += MainWindow_SizeChanged;

        UpdateBackButtonState();
        SelectNavigationItem("Dashboard");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        private set
        {
            if (!string.Equals(_currentPageTitle, value, StringComparison.Ordinal))
            {
                _currentPageTitle = value;
                PageTitleTextBlock.Text = value;
                PageSubtitleTextBlock.Text = GetPageSubtitle(value);
                OnPropertyChanged();
            }
        }
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated;
        InitializeNavigationOnce();
    }

    private void ContentFrame_Loaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Loaded -= ContentFrame_Loaded;
        InitializeNavigationOnce();
    }

    private void InitializeNavigationOnce()
    {
        if (_isNavigationReady)
        {
            return;
        }

        _navigationService.Initialize(ContentFrame);
        _navigationService.Register("Dashboard", typeof(DashboardPage));
        _navigationService.Register("Products", typeof(ProductsPage));
        _navigationService.Register("ProductEdit", typeof(ProductEditPage));
        _navigationService.Register("Orders", typeof(OrdersPage));
        _navigationService.Register("Customers", typeof(CustomersPage));
        _navigationService.Register("Reports", typeof(ReportsPage));
        _navigationService.Register("GraphQL", typeof(GraphQlPage));
        _navigationService.Register("Plugins", typeof(PluginsPage));
        _navigationService.Register("Settings", typeof(SettingsPage));
        _isNavigationReady = true;

        var targetScreen = NormalizeStartupScreen(App.Current.Services.SettingsService.CurrentSettings.LastOpenedScreen);
        if (!_navigationService.Navigate(targetScreen))
        {
            _navigationService.Navigate("Dashboard");
        }

        _ = ShowLicenseGateIfNeededAsync();
        _ = ShowOnboardingIfNeededAsync();
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        WriteInitializeComponentExceptionLog(e.Exception);
        e.Handled = true;

        CurrentPageTitle = "Startup Error";
        ContentFrame.Content = new Border
        {
            Margin = new Thickness(28),
            Padding = new Thickness(24),
            Background = GetBrush("PageSurfaceBrush"),
            BorderBrush = GetBrush("CardStrokeBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(24),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "This page could not be loaded.",
                        FontSize = 24,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        TextWrapping = TextWrapping.WrapWholeWords
                    },
                    new TextBlock
                    {
                        Text = e.Exception.GetBaseException().Message,
                        Foreground = GetBrush("DangerBrush"),
                        TextWrapping = TextWrapping.WrapWholeWords
                    }
                }
            }
        };
    }

    private async Task ShowLicenseGateIfNeededAsync()
    {
        var state = await App.Current.Services.LicenseService.GetStateAsync();
        if (state.CanUseFullApp || ContentFrame.XamlRoot is null)
        {
            return;
        }

        var codeBox = new TextBox { PlaceholderText = "Activation code" };
        var content = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = "Enter a license code to activate 1 month, 1 year, or lifetime access.",
                    TextWrapping = TextWrapping.WrapWholeWords
                },
                new TextBlock
                {
                    Text = $"Demo codes: {LicenseService.DemoOneMonthCode} | {LicenseService.DemoOneYearCode} | {LicenseService.DemoLifetimeCode}",
                    TextWrapping = TextWrapping.WrapWholeWords
                },
                codeBox
            }
        };
        var dialog = new ContentDialog
        {
            Title = "Trial expired",
            Content = content,
            PrimaryButtonText = "Activate",
            CloseButtonText = "Exit",
            XamlRoot = ContentFrame.XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            var result = await App.Current.Services.LicenseService.ActivateAsync(codeBox.Text);
            if (!result.Success)
            {
                App.Current.ShowLoginWindow();
            }
        }
        else
        {
            App.Current.ShowLoginWindow();
        }
    }

    private async Task ShowOnboardingIfNeededAsync()
    {
        var onboarding = App.Current.Services.OnboardingService;
        if (onboarding.IsCompleted || ContentFrame.XamlRoot is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Welcome to MyShop POS",
            Content = "Use Dashboard for store health, Products for catalog work, Orders for sales, Customers for profiles and history, Reports for revenue, GraphQL for API demos, Plugins for extensions, and Settings for app configuration.",
            PrimaryButtonText = "Start",
            CloseButtonText = "Skip",
            XamlRoot = ContentFrame.XamlRoot
        };
        await dialog.ShowAsync();
        onboarding.Complete();
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        var isCompact = args.Size.Width < 760;
        ShellNavigationColumn.Width = isCompact ? new GridLength(72) : new GridLength(280);
        ShellNavigation.Margin = isCompact ? new Thickness(8) : new Thickness(16);
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isNavigationReady ||
            sender is not Button button ||
            button.Tag is not string key)
        {
            return;
        }

        _navigationService.Navigate(key);
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.XamlRoot is not null)
        {
            var dialog = new ContentDialog
            {
                Title = "Log out",
                Content = "Sign out of the current session. Choose Clear saved login if this PC should not auto-login next time.",
                PrimaryButtonText = "Log out",
                SecondaryButtonText = "Clear saved login",
                CloseButtonText = "Cancel",
                XamlRoot = ContentFrame.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                return;
            }

            if (result == ContentDialogResult.Secondary)
            {
                await App.Current.Services.AuthenticationService.ClearCredentialsAsync();
            }
        }

        App.Current.ShowLoginWindow();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        UpdateBackButtonState();

        var pageKey = GetPageKey(e.SourcePageType);
        CurrentPageTitle = pageKey switch
        {
            "ProductEdit" => "Product Editor",
            _ => pageKey
        };

        var selectionKey = pageKey == "ProductEdit" ? "Products" : pageKey;
        SelectNavigationItem(selectionKey);
        RefreshContentLayout();
    }

    private void UpdateBackButtonState()
    {
        BackButton.IsEnabled = ContentFrame.CanGoBack;
        BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SelectNavigationItem(string key)
    {
        _currentNavigationSelection = key;

        foreach (var pair in _navigationButtons)
        {
            var isSelected = string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase);
            ApplyNavigationButtonState(pair.Value, isSelected);
        }

        ApplyNavigationButtonState(LogoutButton, false, isDanger: true);
    }

    private void ApplyNavigationButtonState(Button button, bool isSelected, bool isDanger = false)
    {
        button.Background = isSelected
            ? GetBrush("SidebarSelectedBrush")
            : new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        button.BorderBrush = isSelected
            ? GetBrush("SidebarBorderBrush")
            : new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        var foreground = isDanger && !isSelected
            ? GetBrush("HighlightBrush")
            : GetBrush("SidebarTextBrush");

        button.Foreground = foreground;
        button.Opacity = button.IsEnabled ? 1 : 0.65;
        UpdateNavigationContentForeground(button.Content as DependencyObject, foreground);
    }

    private void RegisterNavigationHoverStates()
    {
        foreach (var button in _navigationButtons.Values.Append(LogoutButton))
        {
            button.PointerEntered += NavigationButton_PointerEntered;
            button.PointerExited += NavigationButton_PointerExited;
        }
    }

    private void NavigationButton_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button || IsSelectedNavigationButton(button))
        {
            return;
        }

        button.Background = GetBrush("SidebarHoverBrush");
        button.BorderBrush = GetBrush("SidebarBorderBrush");
    }

    private void NavigationButton_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var isDanger = ReferenceEquals(button, LogoutButton);
        ApplyNavigationButtonState(button, IsSelectedNavigationButton(button), isDanger);
    }

    private bool IsSelectedNavigationButton(Button button)
    {
        return button.Tag is string key &&
               string.Equals(key, _currentNavigationSelection, StringComparison.OrdinalIgnoreCase);
    }

    private static void UpdateNavigationContentForeground(DependencyObject? element, Brush foreground)
    {
        if (element is null)
        {
            return;
        }

        if (element is TextBlock textBlock)
        {
            textBlock.Foreground = foreground;
        }
        else if (element is FontIcon fontIcon)
        {
            fontIcon.Foreground = foreground;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(element);
        for (var index = 0; index < childCount; index++)
        {
            UpdateNavigationContentForeground(VisualTreeHelper.GetChild(element, index), foreground);
        }
    }

    private static Brush GetBrush(string key)
    {
        return Application.Current.Resources[key] as Brush
               ?? new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    private static string GetPageKey(Type? pageType)
    {
        if (pageType == typeof(DashboardPage))
        {
            return "Dashboard";
        }

        if (pageType == typeof(ProductsPage))
        {
            return "Products";
        }

        if (pageType == typeof(ProductEditPage))
        {
            return "ProductEdit";
        }

        if (pageType == typeof(OrdersPage))
        {
            return "Orders";
        }

        if (pageType == typeof(CustomersPage))
        {
            return "Customers";
        }

        if (pageType == typeof(ReportsPage))
        {
            return "Reports";
        }

        if (pageType == typeof(GraphQlPage))
        {
            return "GraphQL";
        }

        if (pageType == typeof(PluginsPage))
        {
            return "Plugins";
        }

        if (pageType == typeof(SettingsPage))
        {
            return "Settings";
        }

        return "Dashboard";
    }

    private static string NormalizeStartupScreen(string? screen)
    {
        return screen?.Trim() switch
        {
            "Products" => "Products",
            "Orders" => "Orders",
            "Customers" => "Customers",
            "Settings" => "Settings",
            "Dashboard" => "Dashboard",
            _ => "Dashboard"
        };
    }

    private static string GetPageSubtitle(string pageKey)
    {
        return pageKey switch
        {
            "Dashboard" => "Quickly review store status, inventory, and revenue for the day.",
            "Products" => "Manage categories, products, filters, and imported catalog data.",
            "Product Editor" => "Update product details, images, pricing, and storefront specs.",
            "Orders" => "Create and edit orders, track payment status, and keep inventory synchronized.",
            "Customers" => "Customer profiles and purchase history.",
            "Reports" => "Compare revenue, profit, and sales trends across multiple reporting ranges.",
            "GraphQL" => "Run lightweight POS API queries.",
            "Plugins" => "Review and reload local extensions.",
            "Settings" => "Adjust page size and saved-login behavior for the application.",
            _ => "Move between the main work areas of the store application."
        };
    }

    private void RefreshContentLayout()
    {
        ContentFrame.InvalidateMeasure();
        ContentFrame.InvalidateArrange();

        if (ContentFrame.Content is FrameworkElement content)
        {
            content.InvalidateMeasure();
            content.InvalidateArrange();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static void WriteInitializeComponentExceptionLog(Exception ex)
    {
        try
        {
            var builder = new StringBuilder()
                .AppendLine($"Timestamp: {DateTimeOffset.Now:O}")
                .AppendLine("Target: MainWindow.InitializeComponent")
                .AppendLine(ex.ToString())
                .AppendLine();

            foreach (var logPath in GetInitializeComponentLogPaths())
            {
                try
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.AppendAllText(logPath, builder.ToString());
                }
                catch
                {
                    // Continue with the next log path.
                }
            }
        }
        catch
        {
            // Preserve the original exception if logging fails.
        }
    }

    private static IEnumerable<string> GetInitializeComponentLogPaths()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "startup-error.log");
        yield return Path.Combine(Path.GetTempPath(), "ProjectTest-startup-error.log");
        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProjectTest",
            "startup-error.log");
    }
}
