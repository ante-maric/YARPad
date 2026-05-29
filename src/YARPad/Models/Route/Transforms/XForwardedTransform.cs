namespace CodingCell.YARPad;

public sealed record XForwardedTransform() : BuiltInRouteTransform(RouteTransformType.XForwarded)
{
    public required ForwardedTransformAction XForwarded { get; set; }

    public ForwardedTransformAction? For { get; set; }

    public ForwardedTransformAction? Proto { get; set; }

    public ForwardedTransformAction? Host { get; set; }

    public ForwardedTransformAction? Prefix { get; set; }

    public string? HeaderPrefix { get; set; } = "X-Forwarded-";

    public override string GetTransformSummary()
    {
        var parts = new List<string> { $"Default action: {XForwarded}" };

        if (For.HasValue)
            parts.Add($"For: {For.Value}");
        if (Proto.HasValue)
            parts.Add($"Proto: {Proto.Value}");
        if (Host.HasValue)
            parts.Add($"Host: {Host.Value}");
        if (Prefix.HasValue)
            parts.Add($"Prefix: {Prefix.Value}");
        if (!string.IsNullOrWhiteSpace(HeaderPrefix))
            parts.Add($"Header prefix: '{HeaderPrefix}'");

        return string.Join(", ", parts);
    }
}
