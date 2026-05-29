namespace CodingCell.YARPad;

public sealed record ClusterConfigSectionSwitch
{
    public ClusterConfigSection Section { get; set; }

    public bool IsEnabled { get; set; }
}
