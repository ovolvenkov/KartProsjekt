using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KartProsjekt.Views;

public sealed partial class ProjectsPage : Page
{
    public ProjectsPage()
    {
        InitializeComponent();

        DataContext = new ProjectsViewModel();
    }
}