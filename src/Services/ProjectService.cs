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
}