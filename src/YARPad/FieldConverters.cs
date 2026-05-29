using MudBlazor;

namespace CodingCell.YARPad;

internal static class FieldConverters
{
    public static readonly IConverter<Version?, string> Version = CreateVersionConverter();
    public static readonly IConverter<TimeSpan?, string> TimeSpan = CreateTimeSpanConverter();

    private static IConverter<Version?, string> CreateVersionConverter()
    {
        return Conversions.From<Version?, string>(
            x => x?.ToString() ?? string.Empty,
            x =>
            {
                if (string.IsNullOrWhiteSpace(x))
                    return null;

                if (x.EndsWith('.'))
                    x += "0";

                if (System.Version.TryParse(x, out var version))
                    return version;

                throw new FormatException("Version format is invalid.");
            });
    }

    private static IConverter<TimeSpan?, string> CreateTimeSpanConverter()
    {
        return Conversions.From<TimeSpan?, string>(
            x => x?.ToString() ?? string.Empty,
            x =>
            {
                if (string.IsNullOrWhiteSpace(x))
                    return null;

                if (System.TimeSpan.TryParse(x, out var timeSpan))
                    return timeSpan;

                throw new FormatException("TimeSpan format must be hh:mm:ss or d.hh:mm:ss.");
            });
    }
}
