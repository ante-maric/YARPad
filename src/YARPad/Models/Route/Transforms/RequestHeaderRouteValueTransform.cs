namespace CodingCell.YARPad;

public sealed record RequestHeaderRouteValueTransform() : BuiltInRouteTransform(RouteTransformType.RequestHeaderRouteValue)
{
    public required string RequestHeaderRouteValue { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public override string GetTransformSummary()
    {
        if (!string.IsNullOrWhiteSpace(Set))
            return $"Set request header '{RequestHeaderRouteValue}' from route value '{Set}'";

        if (!string.IsNullOrWhiteSpace(Append))
            return $"Append request header '{RequestHeaderRouteValue}' from route value '{Append}'";

        return $"Update request header '{RequestHeaderRouteValue}' from route value";
    }
}
