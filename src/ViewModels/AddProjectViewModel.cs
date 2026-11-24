using CommunityToolkit.Mvvm.ComponentModel;
using KartProsjekt.Models;
using System.Globalization;

namespace KartProsjekt.ViewModels;

public partial class AddProjectViewModel : ObservableObject
{
    public string[] StatusOptions { get; } =
        ["Planned", "Active", "Completed"];

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Budget { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Status { get; set; } = "Planned";

    [ObservableProperty]
    public partial string ValidationMessage { get; set; } = string.Empty;

    public Project? CreateProject()
    {
        var name = Name.Trim();
        if (name.Length == 0)
        {
            ValidationMessage = "Project name is required.";
            return null;
        }

        if (!decimal.TryParse(Budget, NumberStyles.Number, CultureInfo.CurrentCulture, out var budget)
            || budget < 0)
        {
            ValidationMessage = "Enter a valid budget of 0 or greater.";
            return null;
        }

        ValidationMessage = string.Empty;
        return new Project
        {
            Name = name,
            Description = Description.Trim(),
            Budget = budget,
            Status = Status
        };
    }
}
