using KartProsjekt.Models;
using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KartProsjekt.Views;

public sealed partial class AddProjectDialog : ContentDialog
{
    private readonly AddProjectViewModel _viewModel = new();

    public Project? CreatedProject { get; private set; }

    public AddProjectDialog()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    public static async Task<Project?> ShowForAsync(XamlRoot xamlRoot)
    {
        var dialog = new AddProjectDialog
        {
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary
            ? dialog.CreatedProject
            : null;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        CreatedProject = _viewModel.CreateProject();
        if (CreatedProject is null)
        {
            args.Cancel = true;
        }
    }
}
