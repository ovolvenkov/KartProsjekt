using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KartProsjekt.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += DashboardPage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectsViewModel
            ?? throw new InvalidOperationException("DashboardPage requires a ProjectsViewModel.");
    }

    private async void AddProject_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);
        if (project is not null && DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.AddProjectAsync(project);
        }
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }
}