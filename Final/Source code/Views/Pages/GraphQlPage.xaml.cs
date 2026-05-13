using Microsoft.UI.Xaml.Controls;
using ProjectTest.ViewModels;

namespace ProjectTest.Views.Pages;

public sealed partial class GraphQlPage : Page
{
    public GraphQlPage()
    {
        ViewModel = new GraphQlViewModel(App.Current.Services.GraphQlPosService);
        InitializeComponent();
        DataContext = ViewModel;
    }

    public GraphQlViewModel ViewModel { get; }
}
