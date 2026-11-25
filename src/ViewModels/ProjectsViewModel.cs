using CommunityToolkit.Mvvm.ComponentModel;
using KartProsjekt.Models;
using System.Collections.ObjectModel;
using KartProsjekt.Services;

namespace KartProsjekt.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;
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

    public ProjectsViewModel(ProjectService projectService)
    {
        Projects.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalProjects));
        _projectService = projectService;
    }

    // Saves a new project to SQL Server and updates the UI collection.
    public async Task AddProjectAsync(Project project)
    {
        await _projectService.AddAsync(project);

        Projects.Add(project);
    }

    // Loads projects from SQL Server and updates the UI collection.

    public async Task LoadProjectsAsync()
    {
        var projects = await _projectService.GetAllAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }
    }
}
