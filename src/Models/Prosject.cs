
using CommunityToolkit.Mvvm.ComponentModel;

namespace KartProsjekt.Models;

public partial class Project : ObservableObject
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _budget;

    [ObservableProperty]
    private string _status = "Planned";

    // Project identification and classification.
    [ObservableProperty]
    private string _projectNumber = string.Empty;

    [ObservableProperty]
    private string _municipality = string.Empty;

    [ObservableProperty]
    private string _projectManager = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    // Project financial information.
    [ObservableProperty]
    private decimal _actualCost;

    // Project priority and completion progress.
    [ObservableProperty]
    private int _priority = 2;

    [ObservableProperty]
    private int _progress;

    // Project schedule and creation date.
    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    // Project archive status.
    [ObservableProperty]
    private bool _isArchived;
}
