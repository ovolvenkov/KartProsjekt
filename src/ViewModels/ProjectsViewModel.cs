using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KartProsjekt.Models;
using System.Collections.ObjectModel;

namespace KartProsjekt.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    public int TotalProjects => Projects.Count;

    public ObservableCollection<Project> Projects { get; } = new()
    {
        new Project
        {
            Name = "FKB Quality Improvement",
            Description = "Improving geographical data quality",
            Budget = 150000,
            Status = "Active"
        },

        new Project
        {
            Name = "Ortho Photo Control",
            Description = "Quality control of orthophoto data",
            Budget = 85000,
            Status = "Planned"
        },

        new Project
        {
            Name = "Road Network Update",
            Description = "Updating road network information",
            Budget = 120000,
            Status = "Completed"
        }
    };

    public ProjectsViewModel()
    {
        Projects.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalProjects));
    }

    [RelayCommand]
    private void AddProject()
    {
        var project = new Project
        {
            Name = "New Project",
            Description = "New project description",
            Budget = 50000,
            Status = "Planned"
        };

        Projects.Add(project);
    }
}
