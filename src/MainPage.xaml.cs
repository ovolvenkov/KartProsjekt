using KartProsjekt.Services;
using KartProsjekt.ViewModels;
using KartProsjekt.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KartProsjekt;

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly ProjectsViewModel _projectsViewModel = App.Services.GetRequiredService<ProjectsViewModel>();

    public MainPage()
    {
        InitializeComponent();

        var settings = AppSettingsService.Instance;
        settings.SettingsChanged += OnSettingsChanged;
        ApplySettings(settings);

        // Select the default page
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage), _projectsViewModel);
    }

    private void OnSettingsChanged(object? sender, EventArgs args)
    {
        ApplySettings(AppSettingsService.Instance);
    }

    private void ApplySettings(AppSettingsService settings)
    {
        NavView.PaneDisplayMode = settings.IsCompactNavigation
            ? NavigationViewPaneDisplayMode.LeftCompact
            : NavigationViewPaneDisplayMode.Left;
    }

    private void NavView_SelectionChanged(
       NavigationView sender,
       NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is not NavigationViewItem selectedItem)
            return;

        switch (selectedItem.Tag?.ToString())
        {
            case "Dashboard":
                ContentFrame.Navigate(typeof(DashboardPage), _projectsViewModel);
                break;
            case "Projects":
                ContentFrame.Navigate(typeof(ProjectsPage), _projectsViewModel);
                break;
            case "ProjectTable":
                ContentFrame.Navigate(
                    typeof(ProjectTablePage),
                    App.Services.GetRequiredService<ProjectTableViewModel>());
                break;
            case "SyncfusionDataGrid":
                ContentFrame.Navigate(typeof(SyncfusionDataGridPage), _projectsViewModel);
                break;
            case "Tasks":
                ContentFrame.Navigate(typeof(TasksPage));
                break;
            case "Employees":
                ContentFrame.Navigate(typeof(EmployeesPage));
                break; 
            case "Settings":
                ContentFrame.Navigate(typeof(SettingsPage));
                break;
        }
    }
}
