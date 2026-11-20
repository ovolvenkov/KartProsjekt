using KartProsjekt.Services;
using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KartProsjekt.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(AppSettingsService.Instance);
    }
}
