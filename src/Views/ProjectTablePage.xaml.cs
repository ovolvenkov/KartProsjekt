using Microsoft.UI.Dispatching;
using KartProsjekt.ViewModels;
using KartProsjekt.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Tabular;
using Microsoft.UI.Xaml.Navigation;

namespace KartProsjekt.Views;

public sealed partial class ProjectTablePage : Page
{
    private readonly DispatcherQueueTimer _searchDebounceTimer;
    private Project? _editingProject;
    private string? _editingPropertyName;
    private object? _editingOriginalValue;
    private bool _isActive;

    public ProjectTablePage()
    {
        InitializeComponent();

        // Debounces search input on the UI dispatcher before applying the table filter.
        _searchDebounceTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(250);
        _searchDebounceTimer.IsRepeating = false;
        _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

        // Loads the shared collection only if a sibling page has not loaded it yet.
        Loaded += ProjectTablePage_Loaded;
    }

    // Receives the shared ViewModel from the main navigation host.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as ProjectTableViewModel
            ?? throw new InvalidOperationException("ProjectTablePage requires a ProjectTableViewModel.");
        _isActive = true;
        ProjectTable.ItemsSource = ((ProjectTableViewModel)DataContext).TableSource;
        ((ProjectTableViewModel)DataContext).Activate();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _isActive = false;
        _searchDebounceTimer.Stop();
        if (DataContext is ProjectTableViewModel viewModel)
        {
            if (_editingProject is not null && _editingPropertyName is not null)
            {
                viewModel.CancelEdit(_editingProject, _editingPropertyName, _editingOriginalValue);
                ClearPendingEdit();
            }

            viewModel.Deactivate();
        }
        ProjectTable.ItemsSource = null;
        base.OnNavigatedFrom(e);
    }

    // Saves projects through the existing dialog and shared ViewModel.
    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);

        if (project is not null && DataContext is ProjectTableViewModel viewModel)
        {
            await viewModel.AddProjectAsync(project);
        }
    }

    private void ResetSorting_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectTableViewModel viewModel)
        {
            viewModel.ResetSorting();
        }
    }

    // Routes single-column header sorting through TableViewSource.
    private void ProjectTable_Sorting(
        TableView sender,
        TableViewSortingEventArgs args)
    {
        args.Cancel = true;

        if (DataContext is ProjectTableViewModel viewModel &&
            !string.IsNullOrWhiteSpace(args.Column?.SortMemberPath))
        {
            viewModel.SetSingleColumnSort(args.Column.SortMemberPath, args.Direction);
        }
    }

    // Restricts the native edit session to the explicitly supported project properties.
    private void ProjectTable_BeginningEdit(
        TableView sender,
        TableViewBeginningEditEventArgs args)
    {
        if (DataContext is not ProjectTableViewModel viewModel ||
            args.Item is not Project project ||
            string.IsNullOrWhiteSpace(args.Column?.SortMemberPath) ||
            !viewModel.TryBeginEdit(project, args.Column.SortMemberPath, out var originalValue))
        {
            args.Cancel = true;
            return;
        }

        _editingProject = project;
        _editingPropertyName = args.Column.SortMemberPath;
        _editingOriginalValue = originalValue;
    }

    // Defers persistence until TableView has committed its editor binding.
    private void ProjectTable_CellEditEnding(
        TableView sender,
        TableViewCellEditEndingEventArgs args)
    {
        if (_editingProject is null ||
            _editingPropertyName is null ||
            DataContext is not ProjectTableViewModel viewModel)
        {
            return;
        }

        var project = _editingProject;
        var propertyName = _editingPropertyName;
        var originalValue = _editingOriginalValue;

        if (args.EditAction == TableViewEditAction.Cancel)
        {
            viewModel.CancelEdit(project, propertyName, originalValue);
            ClearPendingEdit();
            return;
        }

        if (!sender.DispatcherQueue.TryEnqueue(async () =>
            {
                if (!_isActive)
                {
                    return;
                }

                if (sender.IsEditing)
                {
                    viewModel.ShowEditError("The value is invalid. Correct it or press Escape to cancel.");
                    return;
                }

                if (ReferenceEquals(_editingProject, project) &&
                    _editingPropertyName == propertyName)
                {
                    ClearPendingEdit();
                }

                await viewModel.SaveEditedPropertyAsync(project, propertyName, originalValue);
            }))
        {
            viewModel.CancelEdit(project, propertyName, originalValue);
            viewModel.ShowEditError("The edit could not be saved because the page is closing.");
            ClearPendingEdit();
        }
    }

    private void ClearPendingEdit()
    {
        _editingProject = null;
        _editingPropertyName = null;
        _editingOriginalValue = null;
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private void SearchDebounceTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        _searchDebounceTimer.Stop();
        if (DataContext is ProjectTableViewModel viewModel)
        {
            viewModel.ApplyFilters();
        }
    }

    // Loads the project list from the database when no sibling page has done so.
    private async void ProjectTablePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectTableViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }
}
