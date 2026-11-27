using KartProsjekt.Models;
using KartProsjekt.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Navigation;
using WinUI.TableView;
using Windows.ApplicationModel.DataTransfer;

namespace KartProsjekt.Views;

public sealed partial class CommunityTableViewPage : Page
{
    private Project? _editingProject;
    private string? _editingPropertyName;
    private object? _editingOriginalValue;
    private bool _isActive;
    private readonly DispatcherQueueTimer _searchDebounceTimer;

    public CommunityTableViewPage()
    {
        InitializeComponent();

        // Debounces rebuilding the table's search projection while the user is typing.
        _searchDebounceTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(250);
        _searchDebounceTimer.IsRepeating = false;
        _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

        Loaded += CommunityTableViewPage_Loaded;
    }

    // Receives isolated table state while keeping the application's project collection shared.
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = e.Parameter as CommunityTableViewModel
            ?? throw new InvalidOperationException("CommunityTableViewPage requires a CommunityTableViewModel.");

        var viewModel = (CommunityTableViewModel)DataContext;
        viewModel.Activate();
        CommunityTable.ItemsSource = viewModel.VisibleProjects;
        CommunityTable.SortDescriptions.Clear();
        CommunityTable.SortDescriptions.Add(
            new SortDescription(nameof(Project.Name), SortDirection.Ascending));
        _isActive = true;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _isActive = false;
        _searchDebounceTimer.Stop();
        if (DataContext is CommunityTableViewModel viewModel)
        {
            if (_editingProject is not null && _editingPropertyName is not null)
            {
                viewModel.CancelEdit(
                    _editingProject,
                    _editingPropertyName,
                    _editingOriginalValue);
                ClearPendingEdit();
            }

            viewModel.Deactivate();
        }

        CommunityTable.ItemsSource = null;
        base.OnNavigatedFrom(e);
    }

    // Opens the existing project dialog and saves through the shared project service.
    private async void AddProject_Click(object sender, RoutedEventArgs e)
    {
        var project = await AddProjectDialog.ShowForAsync(XamlRoot);
        if (project is not null && DataContext is CommunityTableViewModel viewModel)
        {
            await viewModel.AddProjectAsync(project);
        }
    }

    // Loads the shared project collection without issuing another database query when already loaded.
    private async void CommunityTableViewPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CommunityTableViewModel viewModel)
        {
            await viewModel.LoadProjectsAsync();
        }
    }

    // Restores the library's default Name ascending sort and its visible sort indicator.
    private void ResetSorting_Click(object sender, RoutedEventArgs e)
    {
        CommunityTable.ClearAllSorting();
        CommunityTable.SortDescriptions.Add(
            new SortDescription(nameof(Project.Name), SortDirection.Ascending));
    }

    // Clears both the library's column filters and the separate global search.
    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        CommunityTable.ClearAllFilters();
        if (DataContext is CommunityTableViewModel viewModel)
        {
            viewModel.SearchText = string.Empty;
            viewModel.RefreshSearchResults();
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    private void SearchDebounceTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        _searchDebounceTimer.Stop();
        if (_isActive && DataContext is CommunityTableViewModel viewModel)
        {
            viewModel.RefreshSearchResults();
        }
    }

    // Restricts edits to fields already permitted and validated by ProjectTableViewModel.
    private void CommunityTable_BeginningEdit(
        object sender,
        TableViewBeginningEditEventArgs args)
    {
        if (DataContext is not CommunityTableViewModel viewModel ||
            args.DataItem is not Project project ||
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

    // Persists only committed edits and restores cancelled edits through the existing coordinator.
    private async void CommunityTable_CellEditEnded(
        object sender,
        TableViewCellEditEndedEventArgs args)
    {
        if (_editingProject is null ||
            _editingPropertyName is null ||
            DataContext is not CommunityTableViewModel viewModel)
        {
            return;
        }

        var project = _editingProject;
        var propertyName = _editingPropertyName;
        var originalValue = _editingOriginalValue;
        ClearPendingEdit();

        if (args.EditAction == TableViewEditAction.Cancel)
        {
            viewModel.CancelEdit(project, propertyName, originalValue);
            return;
        }

        await viewModel.SaveEditedPropertyAsync(project, propertyName, originalValue);
        RefreshTableView(viewModel);
    }

    // Replaces unvalidated bulk paste with a validated single-cell save using the model's real field type.
    private async void CommunityTable_PasteFromClipboard(
        object sender,
        TableViewPasteFromClipboardEventArgs args)
    {
        args.Handled = true;
        if (sender is not TableView table ||
            DataContext is not CommunityTableViewModel viewModel ||
            table.CurrentCellSlot is not { } slot ||
            table.GroupDescriptions.Count > 0)
        {
            if (DataContext is CommunityTableViewModel unavailableViewModel)
            {
                unavailableViewModel.ShowEditError(
                    "Paste requires a focused cell and is unavailable while rows are grouped.");
            }
            return;
        }

        var clipboard = Clipboard.GetContent();
        if (!clipboard.Contains(StandardDataFormats.Text))
        {
            viewModel.ShowEditError("The clipboard does not contain text.");
            return;
        }

        var pastedValue = await clipboard.GetTextAsync();
        if (pastedValue.Contains('\t') || pastedValue.Contains('\n') || pastedValue.Contains('\r'))
        {
            viewModel.ShowEditError(
                "Validated paste currently supports one cell at a time; multi-cell paste is unavailable.");
            return;
        }

        if (slot.Row < 0 ||
            slot.Row >= table.CollectionView.Count ||
            slot.Column < 0 ||
            slot.Column >= table.Columns.Count ||
            table.CollectionView[slot.Row] is not Project project)
        {
            viewModel.ShowEditError("The selected cell cannot accept pasted project data.");
            return;
        }

        var propertyName = table.Columns[slot.Column].SortMemberPath;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            viewModel.ShowEditError("The selected column does not support project editing.");
            return;
        }

        await viewModel.PasteCellAsync(project, propertyName, pastedValue);
        RefreshTableView(viewModel);
    }

    private void RefreshTableView(CommunityTableViewModel viewModel)
    {
        if (!_isActive)
        {
            return;
        }

        viewModel.RefreshSearchResults();
        CommunityTable.RefreshSorting();
        CommunityTable.RefreshFilter();
        CommunityTable.RefreshGrouping();
    }

    private void ClearPendingEdit()
    {
        _editingProject = null;
        _editingPropertyName = null;
        _editingOriginalValue = null;
    }
}
