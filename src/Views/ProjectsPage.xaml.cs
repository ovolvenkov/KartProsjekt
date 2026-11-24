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
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectsViewModel
            ?? throw new InvalidOperationException("ProjectsPage requires a ProjectsViewModel.");
    }

    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);
        if (project is not null && DataContext is ProjectsViewModel viewModel)
        {
            viewModel.AddProject(project);
        }
    }
}