using System.Text.RegularExpressions;

namespace Weasel.Tools.Extensions.Common;

public static class StringExtensions
{
    public static string? EnsureFirstUpper(this string? value)
        => string.IsNullOrEmpty(value) ? value : EnsureFirstUpperNonNull(value);

    public static string EnsureFirstUpperNonNull(this string value)
        => $"{char.ToUpper(value[0])}{value[1..]}";

    public static string? EmptyStringToNull(this string? value)
        => string.IsNullOrEmpty(value) ? null : value;

    public static string TrimSpaces(this string value)
        => new Regex("[ ]{2,}", RegexOptions.None).Replace(value, " ").Trim();

    public static string ClearTypeName(this string name)
    {
        int index = name.LastIndexOf(".") + 1;
        return index == 0 || index >= name.Length ? name : name.Substring(index);
    }
    public static string Crop(this string value, int lengthWithEndCharacters, string? endCharacters = "...")
    {
        if (value.Length <= lengthWithEndCharacters)
        {
            return value;
        }
        return $"{value.Substring(0, lengthWithEndCharacters - endCharacters?.Length ?? 0)}{endCharacters}";
    }
}
