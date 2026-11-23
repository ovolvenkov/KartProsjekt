using KartProsjekt.ViewModels;
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
}