namespace CodingCell.YARPad;

public sealed record ResponseHeaderTransform() : BuiltInRouteTransform(RouteTransformType.ResponseHeader)
{
    public required string ResponseHeader { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public ResponseTransformCondition? When { get; set; }

    public override string GetTransformSummary() =>
        DescribeSetAppend(ResponseHeader, Set, Append, "response header", When);
}
