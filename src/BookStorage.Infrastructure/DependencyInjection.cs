using BookStorage.Core.Interfaces.Infrastructure;
using BookStorage.Infrastructure.Configuration;
using BookStorage.Infrastructure.Persistence;
using BookStorage.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookStorage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName));
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

#region Database
        var db = configuration.GetSection("Database");
        var provider = db["Provider"] ?? "SQLite";
        var connectionString = db["ConnectionString"] ?? "Data Source=bookstorage.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                throw new NotSupportedException($"Database provider '{provider}' is not supported.");
            }
        });
#endregion

        return services;
    }
}