namespace ProjectTest.Views;

public partial class MainWindow
{
    private void CustomerMenu_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(ProjectTest.Views.Pages.CustomersPage));
    }
}
