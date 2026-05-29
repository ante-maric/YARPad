namespace CodingCell.YARPad;

public sealed record RequestHeaderRemoveTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeaderRemove)
{
    public required string RequestHeaderRemove { get; set; }

    public override string GetTransformSummary() =>
        $"Remove request header '{RequestHeaderRemove}'";
}
