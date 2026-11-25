using System;
using KartProsjekt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KartProsjekt.ViewModels;

namespace KartProsjekt.Services;

public static class ServiceConfiguration
{
    public static IServiceProvider Configure()
    {
        var services = new ServiceCollection();

        // Registers the project database service.
        services.AddSingleton<ProjectService>();

        // Registers a shared ViewModel for Dashboard and Projects.
        services.AddSingleton<ProjectsViewModel>();

        var connectionString = Environment.GetEnvironmentVariable(
            "KARTPROSJEKT_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Connection string is missing.");

        services.AddSingleton(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options);

        return services.BuildServiceProvider();
    }
}