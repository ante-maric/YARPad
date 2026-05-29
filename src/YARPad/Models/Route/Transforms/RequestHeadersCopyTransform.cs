namespace CodingCell.YARPad;

public sealed record RequestHeadersCopyTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeadersCopy)
{
    public bool RequestHeadersCopy { get; set; }

    public override string GetTransformSummary() =>
        RequestHeadersCopy ? "Copy all incoming request headers" : "Do not copy incoming request headers";
}
