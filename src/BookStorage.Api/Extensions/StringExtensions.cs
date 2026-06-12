namespace BookStorage.Api.Extensions;

public static class StringExtensions
{
    public static string ToNonEmptyString(this Guid value)
    {
        return value == Guid.Empty ? string.Empty : value.ToString();
    }
    
    public static string? ToNullString(this Guid? value)
    {
        return value?.ToString();
    }
}