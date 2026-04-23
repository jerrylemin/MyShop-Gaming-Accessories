using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
            ["Reports"] = ReportsButton,
            ["Settings"] = SettingsButton
        };

        HeaderDateTextBlock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy");
        PageTitleTextBlock.Text = CurrentPageTitle;
        PageSubtitleTextBlock.Text = GetPageSubtitle(CurrentPageTitle);

        ContentFrame.Navigated += ContentFrame_Navigated;
        Activated += MainWindow_Activated;

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

        _navigationService.Initialize(ContentFrame);
        _navigationService.Register("Dashboard", typeof(DashboardPage));
        _navigationService.Register("Products", typeof(ProductsPage));
        _navigationService.Register("ProductEdit", typeof(ProductEditPage));
        _navigationService.Register("Orders", typeof(OrdersPage));
        _navigationService.Register("Reports", typeof(ReportsPage));
        _navigationService.Register("Settings", typeof(SettingsPage));
        _isNavigationReady = true;

        var targetScreen = App.Current.Services.SettingsService.CurrentSettings.LastOpenedScreen;
        if (!_navigationService.Navigate(targetScreen))
        {
            _navigationService.Navigate("Dashboard");
        }
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

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
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
            ? new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            : new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        button.Foreground = isDanger && !isSelected
            ? GetBrush("HighlightBrush")
            : GetBrush("SidebarTextBrush");
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

        if (pageType == typeof(ReportsPage))
        {
            return "Reports";
        }

        if (pageType == typeof(SettingsPage))
        {
            return "Settings";
        }

        return "Dashboard";
    }

    private static string GetPageSubtitle(string pageKey)
    {
        return pageKey switch
        {
            "Dashboard" => "Theo dõi nhanh tình hình cửa hàng, tồn kho và doanh thu trong ngày.",
            "Products" => "Quản lý danh mục, sản phẩm, bộ lọc và nguồn dữ liệu nhập khẩu.",
            "Product Editor" => "Cập nhật thông tin sản phẩm, hình ảnh và thông số trưng bày.",
            "Orders" => "Tạo và chỉnh sửa đơn hàng, theo dõi trạng thái thanh toán và đồng bộ tồn kho.",
            "Reports" => "Quan sát doanh thu theo nhiều chu kỳ và xu hướng bán hàng của cửa hàng.",
            "Settings" => "Tùy chỉnh cách hiển thị dữ liệu và hành vi đăng nhập cho ứng dụng.",
            _ => "Điều hướng giữa các khu vực làm việc chính của cửa hàng."
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
            content.UpdateLayout();
        }

        ContentFrame.UpdateLayout();
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
