using System.Data.Common;
using Microsoft.Extensions.Options;

namespace BookStorage.Infrastructure.Configuration;

public class ValidateFileStorage : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options)
    {
        var errors = new List<string>();

        if (!Utils.IsValidPath(options.BooksPath, out var error)) {
            errors.Add(error);
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(string.Join("; ", errors));
    }
}

public class ValidateDatabaseOptions : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        var errors = new List<string>();
        try
        {
            var connString = ParseConnectionString(options.ConnectionString);

            switch (options.Provider.ToLowerInvariant())
            {
                case "sqlite":
                    var path = Path.GetDirectoryName(connString["Data Source"]);
                    if (!string.IsNullOrWhiteSpace(path) && !Utils.IsValidPath(path, out var error))
                    {
                        errors.Add(error);
                    }
                    break;
            }
        }
        catch (Exception ex) {
            errors.Add(ex.Message);
        }
        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(string.Join("; ", errors));
    }

    private static Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in builder.Keys)
        {
            result[key] = builder[key].ToString() ?? string.Empty;
        }

        return result;
    }
}

static class Utils
{
    public static bool IsValidPath(string path, out string error)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Path cannot be null or empty";
            return false;
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            error = $"Path contains invalid characters: {path}";
            return false;
        }

        error = "";
        return true;
    }
}