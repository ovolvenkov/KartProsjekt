using System.Collections.Generic;
using System.Threading.Tasks;
using KartProsjekt.Data;
using KartProsjekt.Models;
using Microsoft.EntityFrameworkCore;

namespace KartProsjekt.Services;

public class ProjectService
{
    private readonly DbContextOptions<AppDbContext> _options;

    public ProjectService(DbContextOptions<AppDbContext> options)
    {
        _options = options;
    }

    public async Task<List<Project>> GetAllAsync()
    {
        await using var context = new AppDbContext(_options);

        return await context.Projects
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await using var context = new AppDbContext(_options);

        context.Projects.Add(project);

        await context.SaveChangesAsync();
    }

    // Marks only the validated property as modified in a short-lived context.
    public async Task UpdatePropertyAsync(Project project, string propertyName)
    {
        var editableProperties = new[]
        {
            nameof(Project.Name),
            nameof(Project.Municipality),
            nameof(Project.ProjectManager),
            nameof(Project.Category),
            nameof(Project.Status),
            nameof(Project.Budget),
            nameof(Project.ActualCost),
            nameof(Project.Priority),
            nameof(Project.Progress)
        };

        if (!editableProperties.Contains(propertyName, StringComparer.Ordinal))
        {
            throw new ArgumentException("The selected project property cannot be edited.", nameof(propertyName));
        }

        await using var context = new AppDbContext(_options);
        var entry = context.Projects.Attach(project);
        entry.Property(propertyName).IsModified = true;

        if (await context.SaveChangesAsync() == 0)
        {
            throw new InvalidOperationException("The project could not be found in the database.");
        }
    }
}