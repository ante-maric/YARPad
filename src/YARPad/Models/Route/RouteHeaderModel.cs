using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public sealed record RouteHeaderModel
{
    public required string Name { get; set; }

    public List<string> Values { get; set; } = [];

    public HeaderMatchMode Mode { get; set; }

    public bool IsCaseSensitive { get; set; }
}
