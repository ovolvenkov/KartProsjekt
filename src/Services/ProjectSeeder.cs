using KartProsjekt.Data;
using KartProsjekt.Models;
using Microsoft.EntityFrameworkCore;

namespace KartProsjekt.Services;

// Generates test projects for DataGrid development and performance testing.
public class ProjectSeeder
{
    private readonly DbContextOptions<AppDbContext> _options;

    public ProjectSeeder(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    // Generates and saves a specified number of test projects.
    public async Task SeedAsync(int count = 20)
    {
        await using var context = new AppDbContext(_options);

        var random = new Random();

        // Defines sample values for project generation.
        string[] municipalities =
        [
            "Hamar", "Løten", "Elverum", "Tolga",
            "Os", "Rendalen", "Ringebu", "Lillehammer"
        ];

        string[] categories =
        [
            "Ortofoto", "FKB", "NVDB", "Development", "Other"
        ];

        string[] managers =
        [
            "Test Manager A",
            "Test Manager B",
            "Test Manager C",
            "Test Manager D"
        ];

        string[] statuses =
        [
            "Planned", "Active", "Completed",
            "On Hold", "Cancelled"
        ];

        var projects = new List<Project>();

        // Creates projects with varied financial and scheduling data.
        for (int i = 0; i < count; i++)
        {
            var municipality = municipalities[random.Next(municipalities.Length)];
            var category = categories[random.Next(categories.Length)];
            var status = statuses[random.Next(statuses.Length)];

            var budget = random.Next(50000, 2000000);
            var startDate = DateTime.Today.AddDays(random.Next(-365, 180));

            var progress = status switch
            {
                "Completed" => 100,
                "Planned" => 0,
                _ => random.Next(0, 100)
            };

            projects.Add(new Project
            {
                ProjectNumber = $"TEST-{Guid.NewGuid():N}",
                Name = $"{category} {municipality} {i + 1}",
                Description = "Automatically generated test project.",
                Municipality = municipality,
                ProjectManager = managers[random.Next(managers.Length)],
                Category = category,
                Status = status,
                Budget = budget,
                ActualCost = Math.Round(budget * progress / 100m, 2),
                Priority = random.Next(1, 4),
                Progress = progress,
                StartDate = startDate,
                EndDate = startDate.AddDays(random.Next(30, 365)),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 365)),
                IsArchived = random.Next(0, 10) == 0
            });
        }

        // Saves all generated projects in a single SaveChanges operation.
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }
}
