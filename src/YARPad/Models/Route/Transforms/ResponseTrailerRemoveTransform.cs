namespace CodingCell.YARPad;

public sealed record ResponseTrailerRemoveTransform() : BuiltInRouteTransform(RouteTransformType.ResponseTrailerRemove)
{
    public required string ResponseTrailerRemove { get; set; }

    public ResponseTransformCondition? When { get; set; }

    public override string GetTransformSummary() =>
        DescribeRemove(ResponseTrailerRemove, "response trailer", When);
}
