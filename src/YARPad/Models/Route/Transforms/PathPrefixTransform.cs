namespace CodingCell.YARPad;

public sealed record PathPrefixTransform() : BuiltInRouteTransform(RouteTransformType.PathPrefix)
{
    public required string PathPrefix { get; set; }

    public override string GetTransformSummary() => $"Prefix request path with '{PathPrefix}'";
}
