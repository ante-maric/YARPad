using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CodingCell.YARPad;

internal static partial class StringExtensions
{
    extension(string? input)
    {
        public string HumanizeTitle(string? fallbackValue = null) => input?.Humanize(LetterCasing.Title) ?? (fallbackValue ?? string.Empty);

        [return: NotNullIfNotNull(nameof(input))]
        public string? SplitToLines(int maximumLineLength)
        {
            if (input == null)
                return null;

            return Regex.Replace(input, @"(.{1," + maximumLineLength + @"})(?:\s|$)", "$1<br/>");
        }
    }
}
