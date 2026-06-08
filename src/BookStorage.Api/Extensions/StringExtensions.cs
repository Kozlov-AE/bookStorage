namespace BookStorage.Api.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Конвертирует Guid в string, возвращает пустую строку если Guid.Empty
    /// </summary>
    public static string ToNonEmptyString(this Guid value)
    {
        return value == Guid.Empty ? string.Empty : value.ToString();
    }
    
    /// <summary>
    /// Конвертирует nullable Guid в string, возвращает null если null
    /// </summary>
    public static string? ToNullString(this Guid? value)
    {
        return value?.ToString();
    }
}