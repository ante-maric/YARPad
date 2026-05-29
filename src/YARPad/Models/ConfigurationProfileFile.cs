namespace CodingCell.YARPad;

public class ConfigurationProfileFile
{
    public const string CurrentVersion = "1.0";

    public string Version { get; set; } = CurrentVersion;
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required YARPadConfiguration Configuration { get; set; }
}
