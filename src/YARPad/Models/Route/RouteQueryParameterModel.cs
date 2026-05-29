using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public sealed record RouteQueryParameterModel
{
    public required string Name { get; set; }

    public List<string> Values { get; set; } = [];

    public QueryParameterMatchMode Mode { get; set; }

    public bool IsCaseSensitive { get; set; }
}
