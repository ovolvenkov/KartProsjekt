
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
}
