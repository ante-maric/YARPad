namespace CodingCell.YARPad;

public sealed record HttpMethodChangeTransform() : BuiltInRouteTransform(RouteTransformType.HttpMethodChange)
{
    public required string HttpMethodChange { get; set; }

    public required string Set { get; set; }

    public override string GetTransformSummary()
    {
        var source = string.IsNullOrWhiteSpace(HttpMethodChange) ? "current method" : $"'{HttpMethodChange}'";
        return $"Change HTTP method from {source} to '{Set}'";
    }
}
