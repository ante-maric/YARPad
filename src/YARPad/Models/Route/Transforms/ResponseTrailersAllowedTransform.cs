namespace CodingCell.YARPad;

public sealed record ResponseTrailersAllowedTransform() : BuiltInRouteTransform(RouteTransformType.ResponseTrailersAllowed)
{
    public List<string> AllowedTrailers { get; set; } = [];

    public override string GetTransformSummary() =>
        DescribeList("Forward only these response trailers", AllowedTrailers);
}
