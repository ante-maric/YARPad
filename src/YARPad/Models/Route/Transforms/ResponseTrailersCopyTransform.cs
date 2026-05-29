namespace CodingCell.YARPad;

public sealed record ResponseTrailersCopyTransform() : BuiltInRouteTransform(RouteTransformType.ResponseTrailersCopy)
{
    public bool ResponseTrailersCopy { get; set; }

    public override string GetTransformSummary() =>
        ResponseTrailersCopy ? "Copy all response trailers" : "Do not copy response trailers";
}
