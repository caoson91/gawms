namespace UniformWMS.Shared.Extensions;

public static class StringExtensions
{
    public static string ToSlug(this string text)
        => System.Text.RegularExpressions.Regex
            .Replace(text.ToLowerInvariant().Trim(), @"\s+", "-");

    public static bool IsNullOrEmpty(this string? text) => string.IsNullOrEmpty(text);
    public static bool IsNullOrWhiteSpace(this string? text) => string.IsNullOrWhiteSpace(text);
}

public static class DateTimeExtensions
{
    public static string ToVietnamFormat(this DateTime dt) => dt.ToString("dd/MM/yyyy HH:mm");
    public static string ToDateOnly(this DateTime dt) => dt.ToString("dd/MM/yyyy");

    public static DateTime StartOfDay(this DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime EndOfDay(this DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day, 23, 59, 59, DateTimeKind.Utc);
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();
        var attr = field.GetCustomAttributes(
            typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
        return attr?.Name ?? value.ToString();
    }
}
