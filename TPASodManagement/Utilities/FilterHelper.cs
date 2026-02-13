using System.Text.RegularExpressions;

namespace TpaSodManagement.Utilities;

/// <summary>
/// Helper for normalizing filter/search input (trim, collapse spaces, etc.)
/// </summary>
public static class FilterHelper
{
    private static readonly Regex MultipleWhitespace = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes a search/filter string: trims and collapses multiple whitespace into a single space.
    /// </summary>
    public static string NormalizeSearchText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var trimmed = value.Trim();
        return MultipleWhitespace.Replace(trimmed, " ");
    }
}
