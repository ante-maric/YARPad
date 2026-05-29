namespace CodingCell.YARPad;

public sealed record RequestHeaderTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeader)
{
    public required string RequestHeader { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public bool Remove { get; set; }

    public override string GetTransformSummary()
    {
        if (Remove)
            return $"Remove request header '{RequestHeader}'";

        return DescribeSetAppend(RequestHeader, Set, Append, "request header");
    }
}
