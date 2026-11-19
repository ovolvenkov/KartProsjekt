using Microsoft.UI.Xaml.Controls;

namespace KartProsjekt.Views;

public sealed partial class DashboardPage : Page
{
    public MainViewModel ViewModel { get; set; } = new();
    public DashboardPage()
    {
        InitializeComponent();
    }
}