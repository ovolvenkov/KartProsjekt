using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls.Tabular;

namespace KartProsjekt.ViewModels;

public sealed partial class ProjectSortDescriptor : ObservableObject
{
    [ObservableProperty]
    private SortDirection _direction;

    [ObservableProperty]
    private int _priority;

    public string PropertyName { get; }

    public string DisplayName { get; }

    public string StatusText =>
        $"{DisplayName} {(Direction == SortDirection.Ascending ? "↑" : "↓")}";

    public string PriorityText => $"{Priority}. {StatusText}";

    public ProjectSortDescriptor(ProjectSortField field, SortDirection direction, int priority)
    {
        PropertyName = field.PropertyName;
        DisplayName = field.DisplayName;
        Direction = direction;
        Priority = priority;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Direction) || args.PropertyName == nameof(Priority))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(PriorityText));
            }
        };
    }
}

public sealed record ProjectSortField(string PropertyName, string DisplayName);
