namespace CodingCell.YARPad;

public class YARPadConfiguration
{
    public List<RouteModel> Routes { get; set; } = [];

    public List<ClusterModel> Clusters { get; set; } = [];

    public Dictionary<PolicyType, List<PolicyInfo>> Policies { get; set; } = [];

    public List<CustomTransformDefinition> CustomTransforms { get; set; } = [];
}