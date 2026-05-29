namespace CodingCell.YARPad;

public sealed record RequestHeaderOriginalHostTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeaderOriginalHost)
{
    public bool RequestHeaderOriginalHost { get; set; }

    public override string GetTransformSummary() =>
        RequestHeaderOriginalHost ? "Preserve original Host header" : "Use destination Host header";
}
