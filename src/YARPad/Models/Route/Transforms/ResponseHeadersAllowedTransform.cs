namespace CodingCell.YARPad;

public sealed record ResponseHeadersAllowedTransform() : BuiltInRouteTransform(RouteTransformType.ResponseHeadersAllowed)
{
    public List<string> AllowedHeaders { get; set; } = [];

    public override string GetTransformSummary() =>
        DescribeList("Forward only", AllowedHeaders);
}
