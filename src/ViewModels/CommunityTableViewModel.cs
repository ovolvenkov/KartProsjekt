using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using KartProsjekt.Models;
using KartProsjekt.Services;

namespace KartProsjekt.ViewModels;

public partial class CommunityTableViewModel : ObservableObject
{
    private readonly ProjectsViewModel _projectsViewModel;
    private readonly ProjectTableViewModel _editCoordinator;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<Project> VisibleProjects { get; } = new();

    public string StatusMessage => _editCoordinator.StatusMessage;

    public CommunityTableViewModel(
        ProjectsViewModel projectsViewModel,
        ProjectTableViewModel editCoordinator)
    {
        _projectsViewModel = projectsViewModel;
        _editCoordinator = editCoordinator;
        _editCoordinator.PropertyChanged += EditCoordinator_PropertyChanged;
    }

    // Shares the application's one-time database load and existing add-project workflow.
    public Task LoadProjectsAsync() => _projectsViewModel.LoadProjectsAsync();

    public Task AddProjectAsync(Project project) => _projectsViewModel.AddProjectAsync(project);

    // Tracks shared collection updates only while this page is active.
    public void Activate()
    {
        _projectsViewModel.Projects.CollectionChanged -= Projects_CollectionChanged;
        _projectsViewModel.Projects.CollectionChanged += Projects_CollectionChanged;
        RefreshSearchResults();
    }

    public void Deactivate()
    {
        _projectsViewModel.Projects.CollectionChanged -= Projects_CollectionChanged;
    }

    // Rebuilds the search projection without changing the shared database-backed collection.
    public void RefreshSearchResults()
    {
        var search = SearchText.Trim();
        var matches = _projectsViewModel.Projects
            .Where(project => MatchesSearch(project, search))
            .ToArray();

        VisibleProjects.Clear();
        foreach (var project in matches)
        {
            VisibleProjects.Add(project);
        }
    }

    // Reuses the existing table's field allow-list, validation, SQL save, and rollback behavior.
    public bool TryBeginEdit(Project project, string propertyName, out object? originalValue) =>
        _editCoordinator.TryBeginEdit(project, propertyName, out originalValue);

    public Task SaveEditedPropertyAsync(Project project, string propertyName, object? originalValue) =>
        _editCoordinator.SaveEditedPropertyAsync(project, propertyName, originalValue);

    public void CancelEdit(Project project, string propertyName, object? originalValue) =>
        _editCoordinator.CancelEdit(project, propertyName, originalValue);

    public void ShowEditError(string message) => _editCoordinator.ShowEditError(message);

    // Validates a single pasted value before routing it through the same persistence path as cell edits.
    public async Task PasteCellAsync(Project project, string propertyName, string pastedValue)
    {
        if (!TryBeginEdit(project, propertyName, out var originalValue))
        {
            return;
        }

        if (!TryApplyPastedValue(project, propertyName, pastedValue))
        {
            CancelEdit(project, propertyName, originalValue);
            ShowEditError($"The pasted value is invalid for {propertyName}.");
            return;
        }

        await SaveEditedPropertyAsync(project, propertyName, originalValue);
        RefreshSearchResults();
    }

    private bool TryApplyPastedValue(Project project, string propertyName, string value)
    {
        switch (propertyName)
        {
            case nameof(Project.Name):
                project.Name = value;
                return true;
            case nameof(Project.Municipality):
                project.Municipality = value;
                return true;
            case nameof(Project.ProjectManager):
                project.ProjectManager = value;
                return true;
            case nameof(Project.Category):
                if (FindOption(ProjectTableViewModel.EditableCategoryOptions, value) is { } category)
                {
                    project.Category = category;
                    return true;
                }
                return false;
            case nameof(Project.Status):
                if (FindOption(ProjectTableViewModel.EditableStatusOptions, value) is { } status)
                {
                    project.Status = status;
                    return true;
                }
                return false;
            case nameof(Project.Budget):
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var budget))
                {
                    project.Budget = budget;
                    return true;
                }
                return false;
            case nameof(Project.ActualCost):
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var actualCost))
                {
                    project.ActualCost = actualCost;
                    return true;
                }
                return false;
            case nameof(Project.Priority):
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var priority) &&
                    ProjectTableViewModel.EditablePriorityOptions.Contains(priority))
                {
                    project.Priority = priority;
                    return true;
                }
                return false;
            case nameof(Project.Progress):
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var progress) &&
                    progress is >= 0 and <= 100)
                {
                    project.Progress = progress;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private static string? FindOption(IEnumerable<string> options, string value) =>
        options.FirstOrDefault(option =>
            string.Equals(option, value.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool MatchesSearch(Project project, string search)
    {
        if (search.Length == 0)
        {
            return true;
        }

        return Contains(project.ProjectNumber, search)
            || Contains(project.Name, search)
            || Contains(project.Description, search)
            || Contains(project.Municipality, search)
            || Contains(project.ProjectManager, search)
            || Contains(project.Category, search)
            || Contains(project.Status, search)
            || Contains(project.Budget.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.ActualCost.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.Priority.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.Progress.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.StartDate?.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.EndDate?.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.CreatedAt.ToString(CultureInfo.CurrentCulture), search)
            || Contains(project.IsArchived.ToString(), search);
    }

    private static bool Contains(string? value, string search) =>
        value?.Contains(search, StringComparison.CurrentCultureIgnoreCase) == true;

    private void Projects_CollectionChanged(
        object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var search = SearchText.Trim();

        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                AddMatchingProjects(e.NewItems, search);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                RemoveProjects(e.OldItems);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                RemoveProjects(e.OldItems);
                AddMatchingProjects(e.NewItems, search);
                break;
            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                RefreshSearchResults();
                break;
        }
    }

    private void EditCoordinator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProjectTableViewModel.StatusMessage))
        {
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    private void AddMatchingProjects(System.Collections.IList? items, string search)
    {
        if (items is null)
        {
            return;
        }

        foreach (var project in items.OfType<Project>())
        {
            if (MatchesSearch(project, search))
            {
                VisibleProjects.Add(project);
            }
        }
    }

    private void RemoveProjects(System.Collections.IList? items)
    {
        if (items is null)
        {
            return;
        }

        foreach (var project in items.OfType<Project>())
        {
            VisibleProjects.Remove(project);
        }
    }
}
