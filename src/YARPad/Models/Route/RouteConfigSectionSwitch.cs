namespace CodingCell.YARPad;

public sealed record RouteConfigSectionSwitch
{
    public RouteConfigSection Section { get; set; }

    public bool IsEnabled { get; set; }
}