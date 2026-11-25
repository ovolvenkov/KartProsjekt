using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KartProsjekt.Models;
using KartProsjekt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls.Tabular;

namespace KartProsjekt.ViewModels;

public partial class ProjectTableViewModel : ObservableObject
{
    private readonly ProjectsViewModel _projectsViewModel;
    private readonly ProjectService _projectService;
    private readonly HashSet<Guid> _savingProjectIds = new();
    private int _visibleProjectCount;

    [ObservableProperty]
    private ProjectSortField? _selectedSortField;

    [ObservableProperty]
    private ProjectSortDescriptor? _selectedSortDescriptor;

    [ObservableProperty]
    private SortDirection _selectedSortDirection = SortDirection.Ascending;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedMunicipality = AllFilterOption;

    [ObservableProperty]
    private string _selectedCategory = AllFilterOption;

    [ObservableProperty]
    private string _selectedStatus = AllFilterOption;

    [ObservableProperty]
    private string _selectedPriority = AllFilterOption;

    [ObservableProperty]
    private bool _showArchivedProjects;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public const string AllFilterOption = "All";

    public TableViewSource TableSource { get; }

    public ObservableCollection<ProjectSortDescriptor> SortDescriptors { get; } = new();

    public ObservableCollection<string> MunicipalityOptions { get; } = [AllFilterOption];

    public ObservableCollection<string> CategoryOptions { get; } = [AllFilterOption];

    public ObservableCollection<string> StatusOptions { get; } = [AllFilterOption];

    public IReadOnlyList<string> PriorityOptions { get; } =
        [AllFilterOption, "1", "2", "3"];

    public int TotalProjectCount => _projectsViewModel.TotalProjects;

    public int VisibleProjectCount => _visibleProjectCount;

    public string ProjectCountText =>
        $"Showing {VisibleProjectCount} of {TotalProjectCount} projects";

    public IReadOnlyList<ProjectSortField> SortFields { get; } =
    [
        new(nameof(Project.ProjectNumber), "Project Number"),
        new(nameof(Project.Name), "Project Name"),
        new(nameof(Project.Municipality), "Municipality"),
        new(nameof(Project.Category), "Category"),
        new(nameof(Project.ProjectManager), "Project Manager"),
        new(nameof(Project.Budget), "Budget"),
        new(nameof(Project.ActualCost), "Actual Cost"),
        new(nameof(Project.Status), "Status"),
        new(nameof(Project.Priority), "Priority"),
        new(nameof(Project.Progress), "Progress"),
        new(nameof(Project.StartDate), "Start Date"),
        new(nameof(Project.EndDate), "End Date"),
        new(nameof(Project.CreatedAt), "Created At"),
        new(nameof(Project.IsArchived), "Is Archived")
    ];

    public IReadOnlyList<SortDirection> SortDirectionOptions { get; } =
        [SortDirection.Ascending, SortDirection.Descending];

    public static IReadOnlyList<string> EditableCategoryOptions { get; } =
        ["Ortofoto", "FKB", "NVDB", "Development", "Other"];

    public static IReadOnlyList<string> EditableStatusOptions { get; } =
        ["Planned", "Active", "Completed", "On Hold", "Cancelled"];

    public static IReadOnlyList<int> EditablePriorityOptions { get; } = [1, 2, 3];

    public string SortStatusText => SortDescriptors.Count == 0
        ? "Unsorted"
        : string.Join(" → ", SortDescriptors.Select(descriptor => descriptor.StatusText));

    public ProjectTableViewModel(
        ProjectsViewModel projectsViewModel,
        ProjectService projectService)
    {
        _projectsViewModel = projectsViewModel;
        _projectService = projectService;
        TableSource = TableViewSource.From(_projectsViewModel.Projects);
        SortDescriptors.CollectionChanged += SortDescriptors_CollectionChanged;

        // Applies the default order to the view without changing the shared collection.
        ResetSorting();
    }

    public Task LoadProjectsAsync() => _projectsViewModel.LoadProjectsAsync();

    public Task AddProjectAsync(Project project) => _projectsViewModel.AddProjectAsync(project);

    // Replaces the active sort with the column selected by the native header.
    public bool SetSingleColumnSort(string propertyName, SortDirection direction)
    {
        if (direction == SortDirection.None)
        {
            return false;
        }

        var field = SortFields.FirstOrDefault(sortField => sortField.PropertyName == propertyName);
        if (field is null)
        {
            return false;
        }

        SortDescriptors.Clear();
        var descriptor = new ProjectSortDescriptor(field, direction, 1);
        SortDescriptors.Add(descriptor);
        SelectedSortField = field;
        SelectedSortDescriptor = descriptor;
        SelectedSortDirection = direction;
        ApplySortDescriptors();

        return true;
    }

    // Restores the initial Name ascending order.
    public void ResetSorting()
    {
        SortDescriptors.Clear();

        var defaultField = SortFields.First(field => field.PropertyName == nameof(Project.Name));
        var descriptor = new ProjectSortDescriptor(defaultField, SortDirection.Ascending, 1);
        SortDescriptors.Add(descriptor);

        SelectedSortField = defaultField;
        SelectedSortDescriptor = descriptor;
        SelectedSortDirection = SortDirection.Ascending;
        ApplySortDescriptors();
    }

    // Adds a sort level or changes the direction of an existing level without duplicating it.
    [RelayCommand]
    private void AddOrUpdateSort()
    {
        if (SelectedSortField is null)
        {
            return;
        }

        var descriptor = SortDescriptors.FirstOrDefault(
            sort => sort.PropertyName == SelectedSortField.PropertyName);

        if (descriptor is null)
        {
            descriptor = new ProjectSortDescriptor(
                SelectedSortField,
                SelectedSortDirection,
                SortDescriptors.Count + 1);
            SortDescriptors.Add(descriptor);
        }
        else
        {
            descriptor.Direction = SelectedSortDirection;
        }

        SelectedSortDescriptor = descriptor;
        OnPropertyChanged(nameof(SortStatusText));
        ApplySortDescriptors();
    }

    [RelayCommand]
    private void ApplySelectedSortDirection()
    {
        if (SelectedSortDescriptor is null)
        {
            return;
        }

        SelectedSortDescriptor.Direction = SelectedSortDirection;
        OnPropertyChanged(nameof(SortStatusText));
        ApplySortDescriptors();
    }

    [RelayCommand]
    private void RemoveSelectedSort()
    {
        if (SelectedSortDescriptor is null)
        {
            return;
        }

        SortDescriptors.Remove(SelectedSortDescriptor);
        SelectedSortDescriptor = null;
        ApplySortDescriptors();
    }

    [RelayCommand]
    private void MoveSelectedSortUp()
    {
        MoveSelectedSort(-1);
    }

    [RelayCommand]
    private void MoveSelectedSortDown()
    {
        MoveSelectedSort(1);
    }

    private void MoveSelectedSort(int offset)
    {
        var currentIndex = SortDescriptors.IndexOf(SelectedSortDescriptor!);
        var targetIndex = currentIndex + offset;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= SortDescriptors.Count)
        {
            return;
        }

        SortDescriptors.Move(currentIndex, targetIndex);
        ApplySortDescriptors();
    }

    // Applies sort descriptors in priority order, retaining the shared collection order.
    private void ApplySortDescriptors()
    {
        TableSource.ClearSort();

        foreach (var descriptor in SortDescriptors)
        {
            TableSource.Sort(descriptor.PropertyName, descriptor.Direction);
        }
    }

    private void SortDescriptors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        for (var index = 0; index < SortDescriptors.Count; index++)
        {
            SortDescriptors[index].Priority = index + 1;
        }

        OnPropertyChanged(nameof(SortStatusText));
    }

    // Attaches the collection listener only while this table page is active.
    public void Activate()
    {
        _projectsViewModel.Projects.CollectionChanged -= Projects_CollectionChanged;
        _projectsViewModel.Projects.CollectionChanged += Projects_CollectionChanged;
        RefreshFilterOptions();
        RecalculateVisibleProjectCount();
        NotifyProjectCountsChanged();
    }

    public void Deactivate()
    {
        _projectsViewModel.Projects.CollectionChanged -= Projects_CollectionChanged;
    }

    // Applies all active filters as one predicate without changing the current sort.
    public void ApplyFilters()
    {
        TableSource.Filter(item => item is Project project && MatchesFilters(project));
        RecalculateVisibleProjectCount();
        NotifyProjectCountsChanged();
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedMunicipality = AllFilterOption;
        SelectedCategory = AllFilterOption;
        SelectedStatus = AllFilterOption;
        SelectedPriority = AllFilterOption;
        ShowArchivedProjects = false;
        ApplyFilters();
    }

    private bool MatchesFilters(Project project)
    {
        if (!ShowArchivedProjects && project.IsArchived)
        {
            return false;
        }

        if (SelectedMunicipality != AllFilterOption &&
            !string.Equals(project.Municipality, SelectedMunicipality, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (SelectedCategory != AllFilterOption &&
            !string.Equals(project.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (SelectedStatus != AllFilterOption &&
            !string.Equals(project.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (SelectedPriority != AllFilterOption &&
            (!int.TryParse(SelectedPriority, out var priority) || project.Priority != priority))
        {
            return false;
        }

        var search = SearchText.Trim();
        return search.Length == 0
            || Contains(project.ProjectNumber, search)
            || Contains(project.Name, search)
            || Contains(project.Description, search)
            || Contains(project.Municipality, search)
            || Contains(project.ProjectManager, search)
            || Contains(project.Category, search);
    }

    private static bool Contains(string? value, string search) =>
        value?.Contains(search, StringComparison.CurrentCultureIgnoreCase) == true;

    private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            foreach (Project project in e.NewItems)
            {
                AddOption(MunicipalityOptions, project.Municipality);
                AddOption(CategoryOptions, project.Category);
                AddOption(StatusOptions, project.Status);

                if (MatchesFilters(project))
                {
                    _visibleProjectCount++;
                }
            }
        }
        else if (e.Action != NotifyCollectionChangedAction.Move)
        {
            RefreshFilterOptions();
            RecalculateVisibleProjectCount();
        }

        NotifyProjectCountsChanged();
    }

    private void RefreshFilterOptions()
    {
        var projects = _projectsViewModel.Projects;
        ReplaceOptions(MunicipalityOptions, projects.Select(project => project.Municipality));
        ReplaceOptions(CategoryOptions, projects.Select(project => project.Category));
        ReplaceOptions(StatusOptions, projects.Select(project => project.Status));

        if (!MunicipalityOptions.Contains(SelectedMunicipality))
            SelectedMunicipality = AllFilterOption;
        if (!CategoryOptions.Contains(SelectedCategory))
            SelectedCategory = AllFilterOption;
        if (!StatusOptions.Contains(SelectedStatus))
            SelectedStatus = AllFilterOption;
    }

    private static void ReplaceOptions(
        ObservableCollection<string> options,
        IEnumerable<string?> values)
    {
        options.Clear();
        options.Add(AllFilterOption);

        foreach (var value in values
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase))
        {
            options.Add(value!);
        }
    }

    private static void AddOption(ObservableCollection<string> options, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            !options.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            options.Add(value);
        }
    }

    private void RecalculateVisibleProjectCount()
    {
        _visibleProjectCount = _projectsViewModel.Projects.Count(MatchesFilters);
    }

    private void NotifyProjectCountsChanged()
    {
        OnPropertyChanged(nameof(TotalProjectCount));
        OnPropertyChanged(nameof(VisibleProjectCount));
        OnPropertyChanged(nameof(ProjectCountText));
    }

    // Captures the original value and rejects overlapping edits for the same project.
    public bool TryBeginEdit(Project project, string propertyName, out object? originalValue)
    {
        originalValue = null;
        if (!IsEditableProperty(propertyName) || !_savingProjectIds.Add(project.Id))
        {
            StatusMessage = "This project is currently being saved or the field is read-only.";
            return false;
        }

        originalValue = GetPropertyValue(project, propertyName);
        return true;
    }

    // Persists a committed, validated cell and restores its previous value on failure.
    public async Task SaveEditedPropertyAsync(
        Project project,
        string propertyName,
        object? originalValue)
    {
        if (!_savingProjectIds.Contains(project.Id))
        {
            StatusMessage = "The edit session is no longer active.";
            return;
        }

        try
        {
            var currentValue = GetPropertyValue(project, propertyName);
            if (Equals(currentValue, originalValue))
            {
                StatusMessage = "No changes to save.";
                return;
            }

            if (project.GetErrors(propertyName).Cast<object>().Any())
            {
                StatusMessage = string.Join(" ", project.GetErrors(propertyName).Cast<string>());
                RestorePropertyValue(project, propertyName, originalValue);
                RefreshTableView();
                return;
            }

            await _projectService.UpdatePropertyAsync(project, propertyName);
            RefreshTableView();
            StatusMessage = "Project change saved.";
        }
        catch (DbUpdateException ex)
        {
            RestorePropertyValue(project, propertyName, originalValue);
            RefreshTableView();
            StatusMessage = $"Couldn't save the change: {ex.GetBaseException().Message}";
        }
        catch (InvalidOperationException ex)
        {
            RestorePropertyValue(project, propertyName, originalValue);
            RefreshTableView();
            StatusMessage = $"Couldn't save the change: {ex.Message}";
        }
        finally
        {
            _savingProjectIds.Remove(project.Id);
        }
    }

    public void ShowEditError(string message)
    {
        StatusMessage = message;
    }

    public void CancelEdit(Project project, string propertyName, object? originalValue)
    {
        RestorePropertyValue(project, propertyName, originalValue);
        _savingProjectIds.Remove(project.Id);
        RefreshTableView();
        StatusMessage = "Edit cancelled.";
    }

    private static bool IsEditableProperty(string propertyName) =>
        propertyName == nameof(Project.Name)
        || propertyName == nameof(Project.Municipality)
        || propertyName == nameof(Project.ProjectManager)
        || propertyName == nameof(Project.Category)
        || propertyName == nameof(Project.Status)
        || propertyName == nameof(Project.Budget)
        || propertyName == nameof(Project.ActualCost)
        || propertyName == nameof(Project.Priority)
        || propertyName == nameof(Project.Progress);

    private static object? GetPropertyValue(Project project, string propertyName) =>
        propertyName switch
        {
            nameof(Project.Name) => project.Name,
            nameof(Project.Municipality) => project.Municipality,
            nameof(Project.ProjectManager) => project.ProjectManager,
            nameof(Project.Category) => project.Category,
            nameof(Project.Status) => project.Status,
            nameof(Project.Budget) => project.Budget,
            nameof(Project.ActualCost) => project.ActualCost,
            nameof(Project.Priority) => project.Priority,
            nameof(Project.Progress) => project.Progress,
            _ => throw new ArgumentException("The project property cannot be edited.", nameof(propertyName))
        };

    private static void RestorePropertyValue(Project project, string propertyName, object? value)
    {
        switch (propertyName)
        {
            case nameof(Project.Name):
                project.Name = (string?)value ?? string.Empty;
                break;
            case nameof(Project.Municipality):
                project.Municipality = (string?)value ?? string.Empty;
                break;
            case nameof(Project.ProjectManager):
                project.ProjectManager = (string?)value ?? string.Empty;
                break;
            case nameof(Project.Category):
                project.Category = (string?)value ?? string.Empty;
                break;
            case nameof(Project.Status):
                project.Status = (string?)value ?? string.Empty;
                break;
            case nameof(Project.Budget):
                project.Budget = (decimal)value!;
                break;
            case nameof(Project.ActualCost):
                project.ActualCost = (decimal)value!;
                break;
            case nameof(Project.Priority):
                project.Priority = (int)value!;
                break;
            case nameof(Project.Progress):
                project.Progress = (int)value!;
                break;
            default:
                throw new ArgumentException("The project property cannot be edited.", nameof(propertyName));
        }
    }

    private void RefreshTableView()
    {
        RefreshFilterOptions();
        ApplyFilters();
        ApplySortDescriptors();
    }

    partial void OnSelectedMunicipalityChanged(string value) => ApplyFilters();

    partial void OnSelectedCategoryChanged(string value) => ApplyFilters();

    partial void OnSelectedStatusChanged(string value) => ApplyFilters();

    partial void OnSelectedPriorityChanged(string value) => ApplyFilters();

    partial void OnShowArchivedProjectsChanged(bool value) => ApplyFilters();

    partial void OnSelectedSortDescriptorChanged(ProjectSortDescriptor? value)
    {
        if (value is not null)
        {
            SelectedSortField = SortFields.First(field => field.PropertyName == value.PropertyName);
            SelectedSortDirection = value.Direction;
        }
    }
}
