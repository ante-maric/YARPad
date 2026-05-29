namespace CodingCell.YARPad;

internal static class HttpMethodExtensions
{
    private static List<ConfigOption> _allMethods =
        new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete, HttpMethod.Options, HttpMethod.Head, HttpMethod.Trace }
        .Select(x => new ConfigOption(x.Method, x.Method))
        .ToList();

    extension(HttpMethod)
    {
        public static List<ConfigOption> All => _allMethods;
    }
}