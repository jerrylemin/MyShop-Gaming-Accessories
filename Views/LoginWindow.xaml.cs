using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

namespace ProjectTest.Views;

public sealed partial class LoginWindow : Window
{
    public LoginWindow()
    {
        ViewModel = new LoginViewModel(App.Current.Services.AuthenticationService)
        {
            Username = App.Current.Services.AuthenticationService.DefaultUsername
        };

        InitializeComponent();
        RootGrid.DataContext = ViewModel;
        ViewModel.LoginSucceeded += (_, _) => App.Current.ShowMainWindow();
    }

    public LoginViewModel ViewModel { get; }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.Password = passwordBox.Password;
        }
    }

    private void ServerConfigButton_Click(object sender, RoutedEventArgs e)
    {
        App.Current.ShowDatabaseSetupWindow("Configure the PostgreSQL connection used by MyShop Gaming Accessories POS.");
    }
}
