namespace CodingCell.YARPad;

public sealed record ResponseHeadersCopyTransform() : BuiltInRouteTransform(RouteTransformType.ResponseHeadersCopy)
{
    public bool ResponseHeadersCopy { get; set; }

    public override string GetTransformSummary() =>
        ResponseHeadersCopy ? "Copy all response headers" : "Do not copy response headers";
}
