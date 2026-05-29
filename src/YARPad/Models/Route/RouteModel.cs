using System.Text.Json;

namespace CodingCell.YARPad;

public sealed record RouteModel
{
    public required string RouteID { get; set; }

    public string? ClusterID { get; set; }

    public long? MaxRequestBodySize { get; set; }

    public string? AuthorizationPolicy { get; set; }

    public string? RateLimiterPolicy { get; set; }

    public string? OutputCachePolicy { get; set; }

    public TimeSpan? Timeout { get; set; }

    public string? TimeoutPolicy { get; set; }

    public string? CorsPolicy { get; set; }

    public RouteMatchModel Match { get; set; } = new();

    public List<YarpMetadata> Metadata { get; set; } = [];

    public List<RouteTransform> Transforms { get; init; } = [];

    public bool IsEnabled { get; set; } = true;

    public Dictionary<RouteConfigSection, RouteConfigSectionSwitch> SectionSwitches { get; set; } = Enum.GetValues<RouteConfigSection>()
        .Select(x => new RouteConfigSectionSwitch() { Section = x, IsEnabled = x == RouteConfigSection.General && x == RouteConfigSection.Match })
        .ToDictionary(x => x.Section);

    public IEnumerable<RouteConfigSectionSwitch> OptionalSectionSwitches => SectionSwitches.Values
        .Where(x => x.Section != RouteConfigSection.General && x.Section != RouteConfigSection.Match)
        .OrderBy(x => x.Section);

    public RouteModel DeepClone()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<RouteModel>(json)!;
    }
}

