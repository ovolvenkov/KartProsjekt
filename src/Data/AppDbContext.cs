using Microsoft.EntityFrameworkCore;
using KartProsjekt.Models;

namespace KartProsjekt.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
}
