using MudBlazor;

namespace CodingCell.YARPad;

internal static class PatternMaskExtensions
{
    private static PatternMask _versionMask = new PatternMask("0.0");

    extension(PatternMask)
    {
        public static IMask Version => _versionMask;
    }
}