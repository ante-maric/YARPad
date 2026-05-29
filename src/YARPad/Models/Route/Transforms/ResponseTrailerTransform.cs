namespace CodingCell.YARPad;

public sealed record ResponseTrailerTransform() : BuiltInRouteTransform(RouteTransformType.ResponseTrailer)
{
    public required string ResponseTrailer { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public ResponseTransformCondition? When { get; set; }

    public override string GetTransformSummary() =>
        DescribeSetAppend(ResponseTrailer, Set, Append, "response trailer", When);
}
