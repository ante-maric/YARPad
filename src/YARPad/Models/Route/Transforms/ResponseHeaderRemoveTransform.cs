namespace CodingCell.YARPad;

public sealed record ResponseHeaderRemoveTransform() : BuiltInRouteTransform(RouteTransformType.ResponseHeaderRemove)
{
    public required string ResponseHeaderRemove { get; set; }

    public ResponseTransformCondition? When { get; set; }

    public override string GetTransformSummary() =>
        DescribeRemove(ResponseHeaderRemove, "response header", When);
}
