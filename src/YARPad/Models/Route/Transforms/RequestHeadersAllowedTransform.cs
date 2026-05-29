namespace CodingCell.YARPad;

public sealed record RequestHeadersAllowedTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeadersAllowed)
{
    public List<string> AllowedHeaders { get; set; } = [];

    public override string GetTransformSummary() =>
        DescribeList("Forward only", AllowedHeaders);
}
