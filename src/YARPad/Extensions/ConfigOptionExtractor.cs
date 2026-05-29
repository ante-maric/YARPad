namespace CodingCell.YARPad;

internal record ConfigOption(string ID, string Name);

internal static class ConfigOptionExtractor
{
    private static Dictionary<Type, List<ConfigOption>> _options = new();
    
    public static List<ConfigOption> GetOptions(Type type)
    {
        if (!_options.TryGetValue(type, out var options))
        {
            options = type
                .GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Select(x => x.GetValue(null)!.ToString()!)
                .Concat(type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                    .Select(x => x.GetValue(null)!.ToString()!))
                .Select(x => new ConfigOption(x, x.HumanizeTitle()))
                .ToList();

            _options[type] = options;
        }

        return options;
    }
}
