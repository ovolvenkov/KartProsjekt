using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KartProsjekt.Views;

public sealed partial class ProjectTablePage : Page
{
    public ProjectTablePage()
    {
        InitializeComponent();

        // Loads the shared collection only if a sibling page has not loaded it yet.
        Loaded += ProjectTablePage_Loaded;
    }

    // Receives the shared ViewModel from the main navigation host.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectsViewModel
            ?? throw new InvalidOperationException("ProjectTablePage requires a ProjectsViewModel.");
    }

    // Saves projects through the existing dialog and shared ViewModel.
    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);

        if (project is not null && DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.AddProjectAsync(project);
        }
    }

    // Loads the project list from the database when no sibling page has done so.
    private async void ProjectTablePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }
}
