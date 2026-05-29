namespace CodingCell.YARPad;

public sealed record QueryRouteParameterTransform() : BuiltInRouteTransform(RouteTransformType.QueryRouteParameter)
{
    public required string QueryRouteParameter { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public override string GetTransformSummary()
    {
        if (!string.IsNullOrWhiteSpace(Set))
            return $"Set query parameter '{QueryRouteParameter}' from route value '{Set}'";

        if (!string.IsNullOrWhiteSpace(Append))
            return $"Append query parameter '{QueryRouteParameter}' from route value '{Append}'";

        return $"Update query parameter '{QueryRouteParameter}' from route value";
    }
}
