using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ModalCalendarNotification.Data;

/// <summary>
/// Initializes the application database by creating or migrating the schema.
/// Runs on application startup to ensure database is ready.
/// </summary>
public sealed class DatabaseInitializationService
{
    private readonly AppDbContext _dbContext;

    public DatabaseInitializationService(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Ensures the database exists and is up to date with latest migrations.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            Debug.WriteLine("Initializing application database");

            // Get pending migrations
            var pendingMigrations = (await _dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Count > 0)
            {
                Debug.WriteLine($"Found {pendingMigrations.Count} pending migrations");
                foreach (var migration in pendingMigrations)
                {
                    Debug.WriteLine($"  - {migration}");
                }

                // Apply pending migrations
                await _dbContext.Database.MigrateAsync();
                Debug.WriteLine("Applied pending migrations successfully");
            }
            else if (!await _dbContext.Database.CanConnectAsync())
            {
                // Database doesn't exist - create it
                Debug.WriteLine("Database does not exist, creating new database");
                await _dbContext.Database.EnsureCreatedAsync();
                Debug.WriteLine("Database created successfully");
            }
            else
            {
                Debug.WriteLine("Database is up to date");
            }

            Debug.WriteLine("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to initialize database: {ex.Message}");
            throw;
        }
    }
}





