namespace CodingCell.YARPad;

public sealed record PathSetTransform() : BuiltInRouteTransform(RouteTransformType.PathSet)
{
    public required string PathSet { get; set; }

    public override string GetTransformSummary() => $"Set request path to '{PathSet}'";
}
