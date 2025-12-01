using Library.Infrastructure.Data;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Seeds the database with initial data if the NeedSeed configuration is set to true.
    /// This extension method handles database migration and seeding in one call.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="checkConfiguration">Whether to check configuration/environment for NeedSeed flag. Default is true.</param>
    /// <returns>The same web application instance for method chaining</returns>
    public static async Task<WebApplication> SeedDatabaseAsync(
        this WebApplication app,
        bool checkConfiguration = true)
    {
        // Check if seeding should be performed
        var needSeed = !checkConfiguration || ShouldSeedDatabase(app);

        if (!needSeed)
        {
            return app;
        }

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            logger.LogInformation("Database initialization requested. Starting process...");

            var context = services.GetRequiredService<LibraryDbContext>();

            // Apply pending migrations
            await ApplyMigrationsAsync(context, logger);

            // Seed the database
            await SeedDataAsync(services, logger);

            logger.LogInformation("Database initialization completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Checks if database seeding should be performed based on configuration or environment variables.
    /// </summary>
    private static bool ShouldSeedDatabase(WebApplication app)
    {
        var fromConfig = app.Configuration.GetValue<bool>("NeedSeed", false);
        var fromEnv = Environment.GetEnvironmentVariable("NeedSeed")?.ToLower() == "true";

        return fromConfig || fromEnv;
    }

    /// <summary>
    /// Applies pending database migrations.
    /// </summary>
    private static async Task ApplyMigrationsAsync(
        LibraryDbContext context,
        ILogger logger)
    {
        logger.LogInformation("Checking for pending migrations...");

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingMigrationsList = pendingMigrations.ToList();

        if (pendingMigrationsList.Any())
        {
            logger.LogInformation(
                "Found {Count} pending migration(s): {Migrations}",
                pendingMigrationsList.Count,
                string.Join(", ", pendingMigrationsList));

            logger.LogInformation("Applying migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date. No migrations needed");
        }
    }

    /// <summary>
    /// Seeds the database with initial data using the DatabaseSeeder service.
    /// </summary>
    private static async Task SeedDataAsync(
        IServiceProvider services,
        ILogger logger)
    {
        logger.LogInformation("Starting database seeding...");

        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        logger.LogInformation("Database seeding completed");
    }

    /// <summary>
    /// Ensures the database is created and optionally seeds it.
    /// Useful for development and testing environments.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <param name="seed">Whether to seed the database after creation. Default is true.</param>
    /// <returns>The same web application instance for method chaining</returns>
    public static async Task<WebApplication> EnsureDatabaseCreatedAsync(
        this WebApplication app,
        bool seed = true)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            var context = services.GetRequiredService<LibraryDbContext>();

            logger.LogInformation("Ensuring database is created...");
            var created = await context.Database.EnsureCreatedAsync();

            if (created)
            {
                logger.LogInformation("Database created successfully");

                if (seed)
                {
                    await SeedDataAsync(services, logger);
                }
            }
            else
            {
                logger.LogInformation("Database already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring database creation");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Drops the database if it exists and recreates it with seed data.
    /// WARNING: This will delete all existing data!
    /// Should only be used in development/testing environments.
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <returns>The same web application instance for method chaining</returns>
    public static async Task<WebApplication> RecreateAndSeedDatabaseAsync(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            var context = services.GetRequiredService<LibraryDbContext>();

            logger.LogWarning("Recreating database - ALL DATA WILL BE LOST!");

            await context.Database.EnsureDeletedAsync();
            logger.LogInformation("Database deleted");

            await context.Database.MigrateAsync();
            logger.LogInformation("Database recreated with migrations");

            await SeedDataAsync(services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while recreating the database");
            throw;
        }

        return app;
    }
}
