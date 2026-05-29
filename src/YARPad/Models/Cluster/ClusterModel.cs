using System.Text.Json;
using Yarp.ReverseProxy.LoadBalancing;

namespace CodingCell.YARPad;

public sealed record ClusterModel
{
    public required string ClusterID { get; set; }

    public string LoadBalancingPolicy { get; set; } = LoadBalancingPolicies.PowerOfTwoChoices;

    public Dictionary<ClusterConfigSection, ClusterConfigSectionSwitch> SectionSwitches { get; set; } = Enum.GetValues<ClusterConfigSection>()
        .Select(x => new ClusterConfigSectionSwitch() { Section = x, IsEnabled = x == ClusterConfigSection.Destinations })
        .ToDictionary(x => x.Section);

    public IEnumerable<ClusterConfigSectionSwitch> OptionalSectionSwitches => SectionSwitches.Values
        .Where(x => x.Section != ClusterConfigSection.Destinations)
        .OrderBy(x => x.Section);

    public SessionAffinityModel SessionAffinity { get; set; } = new() { AffinityKeyName = string.Empty };

    public HealthCheckModel HealthCheck { get; set; } = new();

    public HttpClientModel HttpClient { get; set; } = new();

    public ForwarderRequestModel HttpRequest { get; set; } = new();

    public List<DestinationModel> Destinations { get; set; } = [];

    public List<YarpMetadata> Metadata { get; set; } = [];

    public ClusterModel DeepClone()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<ClusterModel>(json)!;
    }
}
