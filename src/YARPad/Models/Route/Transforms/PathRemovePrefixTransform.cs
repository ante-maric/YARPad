namespace CodingCell.YARPad;

public sealed record PathRemovePrefixTransform() : BuiltInRouteTransform(RouteTransformType.PathRemovePrefix)
{
    public required string PathRemovePrefix { get; set; }

    public override string GetTransformSummary() => $"Remove path prefix '{PathRemovePrefix}'";
}
