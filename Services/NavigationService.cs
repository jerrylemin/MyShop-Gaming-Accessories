using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ProjectTest.Services;

public class NavigationService
{
    private readonly Dictionary<string, Type> _pageMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly SettingsService _settingsService;
    private Frame? _frame;

    public NavigationService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public string CurrentKey { get; private set; } = "Dashboard";

    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    public void Register(string key, Type pageType)
    {
        _pageMap[key] = pageType;
    }

    public bool Navigate(string key, object? parameter = null, bool persist = true)
    {
        if (_frame is null || !_pageMap.TryGetValue(key, out var pageType))
        {
            return false;
        }

        var navigated = _frame.Navigate(pageType, parameter, new SuppressNavigationTransitionInfo());
        if (!navigated)
        {
            return false;
        }

        CurrentKey = key;
        if (persist)
        {
            var settings = _settingsService.CurrentSettings;
            settings.LastOpenedScreen = key;
            _settingsService.SaveAsync(settings).GetAwaiter().GetResult();
        }

        return true;
    }
}
