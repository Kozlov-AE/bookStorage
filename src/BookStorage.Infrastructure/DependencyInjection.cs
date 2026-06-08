using System.Data.Common;
using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Infrastructure.Configuration;
using BookStorage.Infrastructure.Persistence;
using BookStorage.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStorage.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();

        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
    }
    public static void UseInfrastructure(this IServiceProvider services)
    {
        var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<StorageOptions>>();
        Directory.CreateDirectory(storageOptions.Value.BooksPath);
        logger.LogInformation("Storage directory ensured: {StoragePath}", storageOptions.Value.BooksPath);

        var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
        if (dbOptions.Value.Provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = dbOptions.Value.ConnectionString };
            var dataSource = builder["Data Source"]?.ToString();
            if (!string.IsNullOrEmpty(dataSource))
            {
                var dir = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        try
        {
            logger.LogInformation("Checking database and applying migrations...");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying database migrations");
            throw;
        }
    }
}
