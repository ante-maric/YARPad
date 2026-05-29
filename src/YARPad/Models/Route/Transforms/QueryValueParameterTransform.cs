namespace CodingCell.YARPad;

public sealed record QueryValueParameterTransform() : BuiltInRouteTransform(RouteTransformType.QueryValueParameter)
{
    public required string QueryValueParameter { get; set; }

    public string? Set { get; set; }

    public string? Append { get; set; }

    public override string GetTransformSummary()
    {
        if (!string.IsNullOrWhiteSpace(Set))
            return $"Set query parameter '{QueryValueParameter}' to '{Set}'";

        if (!string.IsNullOrWhiteSpace(Append))
            return $"Append query parameter '{QueryValueParameter}' with '{Append}'";

        return $"Update query parameter '{QueryValueParameter}'";
    }
}
