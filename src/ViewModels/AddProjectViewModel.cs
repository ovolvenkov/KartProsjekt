using CommunityToolkit.Mvvm.ComponentModel;
using KartProsjekt.Models;
using System.Globalization;

namespace KartProsjekt.ViewModels;

public partial class AddProjectViewModel : ObservableObject
{
    // Defines available options for project dropdown controls.
    public string[] StatusOptions { get; } =
        ["Planned", "Active", "Completed", "On Hold", "Cancelled"];

    public string[] CategoryOptions { get; } =
        ["Ortofoto", "FKB", "NVDB", "Development", "Other"];

    public int[] PriorityOptions { get; } =
        [1, 2, 3];

    // General project information.
    [ObservableProperty]
    public partial string ProjectNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Municipality { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProjectManager { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Category { get; set; } = "Other";

    [ObservableProperty]
    public partial string Status { get; set; } = "Planned";

    // Stores financial input as text for decimal validation.
    [ObservableProperty]
    public partial string Budget { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActualCost { get; set; } = "0";

    // Project scheduling and progress information.
    [ObservableProperty]
    public partial DateTimeOffset? StartDate { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? EndDate { get; set; }

    [ObservableProperty]
    public partial int Priority { get; set; } = 2;

    [ObservableProperty]
    public partial double Progress { get; set; } = 0;

    // Controls the project archive status.
    [ObservableProperty]
    public partial bool IsArchived { get; set; }

    // Displays validation errors in the dialog.
    [ObservableProperty]
    public partial string ValidationMessage { get; set; } = string.Empty;

    // Validates form input and creates a new Project entity.
    public Project? CreateProject()
    {
        // Validates the required project name.
        var name = Name.Trim();

        if (name.Length == 0)
        {
            ValidationMessage = "Project name is required.";
            return null;
        }

        // Validates the required project number.
        if (string.IsNullOrWhiteSpace(ProjectNumber))
        {
            ValidationMessage = "Project number is required.";
            return null;
        }

        // Validates and converts the project budget.
        if (!decimal.TryParse(
                Budget,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var budget)
            || budget < 0)
        {
            ValidationMessage = "Enter a valid budget of 0 or greater.";
            return null;
        }

        // Validates and converts the actual project cost.
        if (!decimal.TryParse(
                ActualCost,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var actualCost)
            || actualCost < 0)
        {
            ValidationMessage = "Enter a valid actual cost.";
            return null;
        }

        // Validates the project date range.
        if (StartDate.HasValue &&
            EndDate.HasValue &&
            EndDate.Value.Date < StartDate.Value.Date)
        {
            ValidationMessage = "End date cannot be before start date.";
            return null;
        }

        // Validates progress as a whole percentage.
        if (!double.IsFinite(Progress) ||
            Progress < 0 ||
            Progress > 100 ||
            Progress != Math.Truncate(Progress))
        {
            ValidationMessage = "Progress must be a whole number between 0 and 100.";
            return null;
        }

        // Clears previous validation errors.
        ValidationMessage = string.Empty;

        // Maps validated form values to the database entity.
        return new Project
        {
            ProjectNumber = ProjectNumber.Trim(),
            Name = name,
            Description = Description.Trim(),
            Municipality = Municipality.Trim(),
            ProjectManager = ProjectManager.Trim(),
            Category = Category,
            Status = Status,
            Budget = budget,
            ActualCost = actualCost,
            Priority = Priority,
            Progress = (int)Progress,
            StartDate = StartDate?.Date,
            EndDate = EndDate?.Date,
            CreatedAt = DateTime.UtcNow,
            IsArchived = IsArchived
        };
    }
}