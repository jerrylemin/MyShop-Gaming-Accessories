using ProjectTest.Helpers;

namespace ProjectTest.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentScreenTitle = "Dashboard";

    public string WindowTitle => "MyShop Gaming Accessories POS";

    public string CurrentScreenTitle
    {
        get => _currentScreenTitle;
        set => SetProperty(ref _currentScreenTitle, value);
    }
}
