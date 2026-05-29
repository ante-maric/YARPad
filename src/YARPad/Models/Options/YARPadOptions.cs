namespace CodingCell.YARPad;

public class YARPadOptions
{
    public const string SECTION_NAME = "YARPad";

    public string PathPrefix { get; set; } = "/yarpad";

    /// <summary>
    /// Connection string for the default SQLite database provider.
    /// This is only used when <see cref="ConfigureDbContext"/> is not set.
    /// </summary>
    public string ConnectionString { get; set; } = "DataSource=yarpad.db";

    public bool MultiUserEnabled { get; set; }

    public bool IsLanOnlyAccessDisabled { get; set; }

    /// <summary>
    /// Hostnames that are allowed to serve YARPad.
    /// When empty, only the path prefix is checked.
    /// </summary>
    public HashSet<string> Hosts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Configuration for LAN-only access restriction.
    /// </summary>
    public LanAccessOptions LanAccess { get; set; } = new();

    /// <summary>
    /// Unique identifier for this YARP instance.
    /// If not set, machine name will be used.
    /// </summary>
    public string InstanceID { get; set; } = Environment.MachineName;

    public string GetNormalizedPathPrefix()
    {
        if (string.IsNullOrWhiteSpace(PathPrefix) || PathPrefix == "/")
            return "/";

        return PathPrefix.StartsWith("/") ? PathPrefix.TrimEnd('/') : "/" + PathPrefix.TrimEnd('/');
    }

    public string GetBasePath()
    {
        var basePath = GetNormalizedPathPrefix();
        if (basePath != "/")
            basePath += "/";

        return basePath;
    }
}
