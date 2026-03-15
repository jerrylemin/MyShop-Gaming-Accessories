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
    private readonly Frame _contentFrame = new();
    private readonly ListBox _navigationList = new();
    private readonly Button _backButton = new();
    private readonly TextBlock _pageTitleTextBlock = new();
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

        BuildShell();

        _navigationService = App.Current.Services.NavigationService;
        _pageTitleTextBlock.Text = CurrentPageTitle;
        UpdateBackButtonState();

        _contentFrame.Navigated += ContentFrame_Navigated;
        Activated += MainWindow_Activated;
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
                _pageTitleTextBlock.Text = value;
                OnPropertyChanged();
            }
        }
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        Activated -= MainWindow_Activated;

        _navigationService.Initialize(_contentFrame);
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

    private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (!_isNavigationReady)
        {
            return;
        }

        if (_navigationList.SelectedItem is not ListBoxItem selectedItem ||
            selectedItem.Tag is not string key)
        {
            return;
        }

        _navigationService.Navigate(key);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_contentFrame.CanGoBack)
        {
            _contentFrame.GoBack();
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RefreshContentLayout()
    {
        _contentFrame.InvalidateMeasure();
        _contentFrame.InvalidateArrange();

        if (_contentFrame.Content is FrameworkElement content)
        {
            content.InvalidateMeasure();
            content.InvalidateArrange();
            content.UpdateLayout();
        }

        _contentFrame.UpdateLayout();
    }

    private void UpdateBackButtonState()
    {
        _backButton.IsEnabled = _contentFrame.CanGoBack;
        _backButton.Visibility = _contentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SelectNavigationItem(string key)
    {
        foreach (var item in _navigationList.Items.OfType<ListBoxItem>())
        {
            if (string.Equals(item.Tag as string, key, StringComparison.OrdinalIgnoreCase))
            {
                if (!ReferenceEquals(_navigationList.SelectedItem, item))
                {
                    _navigationList.SelectedItem = item;
                }

                return;
            }
        }
    }

    private void BuildShell()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var leftPanel = new Border
        {
            BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 211, 211, 211)),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(20)
        };

        var leftStack = new StackPanel();
        var titleStack = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
        titleStack.Children.Add(new TextBlock
        {
            FontSize = 24,
            Text = "MyShop Gaming Accessories POS",
            TextWrapping = TextWrapping.Wrap
        });
        titleStack.Children.Add(new TextBlock
        {
            Margin = new Thickness(0, 6, 0, 0),
            FontSize = 12,
            Text = "Gaming accessory inventory, orders, dashboard, and reports",
            TextWrapping = TextWrapping.Wrap
        });
        leftStack.Children.Add(titleStack);

        _navigationList.SelectionChanged += NavigationList_SelectionChanged;
        _navigationList.Items.Add(CreateNavigationItem("Dashboard"));
        _navigationList.Items.Add(CreateNavigationItem("Products"));
        _navigationList.Items.Add(CreateNavigationItem("Orders"));
        _navigationList.Items.Add(CreateNavigationItem("Reports"));
        _navigationList.Items.Add(CreateNavigationItem("Settings"));
        leftStack.Children.Add(_navigationList);

        leftPanel.Child = leftStack;
        root.Children.Add(leftPanel);

        var rightPanel = new Grid
        {
            Padding = new Thickness(24)
        };
        rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        rightPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(rightPanel, 1);

        var headerRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        _backButton.Content = "Back";
        _backButton.Margin = new Thickness(0, 0, 12, 0);
        _backButton.Click += BackButton_Click;
        headerRow.Children.Add(_backButton);

        _pageTitleTextBlock.FontSize = 24;
        _pageTitleTextBlock.VerticalAlignment = VerticalAlignment.Center;
        _pageTitleTextBlock.Text = "Dashboard";
        headerRow.Children.Add(_pageTitleTextBlock);

        rightPanel.Children.Add(headerRow);

        _contentFrame.Margin = new Thickness(0, 24, 0, 0);
        Grid.SetRow(_contentFrame, 1);
        rightPanel.Children.Add(_contentFrame);

        root.Children.Add(rightPanel);
        Content = root;
    }

    private static ListBoxItem CreateNavigationItem(string key)
    {
        return new ListBoxItem
        {
            Content = key,
            Tag = key
        };
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
