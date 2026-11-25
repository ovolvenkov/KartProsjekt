using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KartProsjekt.Views;

public sealed partial class ProjectsPage : Page
{
    public ProjectsPage()
    {
        InitializeComponent();

        // Triggers project loading when the page is loaded.
        Loaded += ProjectsPage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectsViewModel
            ?? throw new InvalidOperationException("ProjectsPage requires a ProjectsViewModel.");
    }

    // Opens the project creation dialog and saves the new project to SQL Server.
    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);

        if (project is not null && DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.AddProjectAsync(project);
        }
    }

    // Loads projects from the database when the Projects page is opened.
    private async void ProjectsPage_Loaded(
    object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }
}