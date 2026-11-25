
﻿using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KartProsjekt.Models;

public partial class Project : ObservableObject, INotifyDataErrorInfo
{
    private readonly Dictionary<string, string> _validationErrors = new();

    public Guid Id { get; private set; } = Guid.NewGuid();

    [NotMapped]
    public bool HasErrors => _validationErrors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _validationErrors.Values.ToArray();
        }

        return _validationErrors.TryGetValue(propertyName, out var error)
            ? new[] { error }
            : Array.Empty<string>();
    }

    partial void OnNameChanging(string value)
    {
        SetValidationError(
            nameof(Name),
            string.IsNullOrWhiteSpace(value) ? "Project name is required." : null);
    }

    partial void OnBudgetChanging(decimal value)
    {
        SetValidationError(
            nameof(Budget),
            value < 0 ? "Budget cannot be negative." : null);
    }

    partial void OnActualCostChanging(decimal value)
    {
        SetValidationError(
            nameof(ActualCost),
            value < 0 ? "Actual cost cannot be negative." : null);
    }

    partial void OnPriorityChanging(int value)
    {
        SetValidationError(
            nameof(Priority),
            value is < 1 or > 3 ? "Priority must be 1, 2, or 3." : null);
    }

    partial void OnProgressChanging(int value)
    {
        SetValidationError(
            nameof(Progress),
            value is < 0 or > 100 ? "Progress must be a whole number from 0 to 100." : null);
    }

    private void SetValidationError(string propertyName, string? error)
    {
        var hadError = _validationErrors.TryGetValue(propertyName, out var previousError);
        if (error is null)
        {
            if (!hadError)
            {
                return;
            }

            _validationErrors.Remove(propertyName);
        }
        else
        {
            if (hadError && previousError == error)
            {
                return;
            }

            _validationErrors[propertyName] = error;
        }

        OnPropertyChanged(nameof(HasErrors));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
