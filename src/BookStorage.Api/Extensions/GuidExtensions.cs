namespace BookStorage.Api.Extensions;

public static class GuidExtensions
{
    /// <summary>
    /// Converts string to Guid, returns Guid.Empty if null or empty
    /// </summary>
    public static Guid ToGuid(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Guid.Empty;

        return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
    }

    public static Guid? ToNullableGuid(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    public static string? ToNullableString(this Guid? value)
    {
        if (value == null || value == Guid.Empty)
        {
            return null;
        }
        return value.ToString();
    }
    public static string? ToNullableString(this Guid value)
    {
        if (value == Guid.Empty)
        {
            return null;
        }
        return value.ToString();
    }
}