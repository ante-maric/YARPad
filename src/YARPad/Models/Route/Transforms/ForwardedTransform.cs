namespace CodingCell.YARPad;

public sealed record ForwardedTransform() : BuiltInRouteTransform(RouteTransformType.Forwarded)
{
    public List<ForwardedTransformNode> Forwarded { get; set; } = [];

    public ForwardedNodeFormat? ForFormat { get; set; }

    public ForwardedNodeFormat? ByFormat { get; set; }

    public ForwardedTransformAction? Action { get; set; }

    public override string GetTransformSummary()
    {
        var nodes = Forwarded.Count == 0 ? "none" : string.Join(", ", Forwarded.Select(x => x.HumanizeEnum()));
        var parts = new List<string> { $"Forwarded header nodes: {nodes}" };

        if (ForFormat.HasValue)
            parts.Add($"ForFormat: {ForFormat.Value.HumanizeEnum()}");
        if (ByFormat.HasValue)
            parts.Add($"ByFormat: {ByFormat.Value.HumanizeEnum()}");
        if (Action.HasValue)
            parts.Add($"Action: {Action.Value}");

        return string.Join(", ", parts);
    }
}
