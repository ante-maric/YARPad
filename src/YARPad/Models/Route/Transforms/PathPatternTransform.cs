namespace CodingCell.YARPad;

public sealed record PathPatternTransform() : BuiltInRouteTransform(RouteTransformType.PathPattern)
{
    public required string PathPattern { get; set; }

    public override string GetTransformSummary() => $"Rewrite path using pattern '{PathPattern}'";
}
