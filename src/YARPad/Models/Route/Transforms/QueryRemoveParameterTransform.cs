namespace CodingCell.YARPad;

public sealed record QueryRemoveParameterTransform() : BuiltInRouteTransform(RouteTransformType.QueryRemoveParameter)
{
    public required string QueryRemoveParameter { get; set; }

    public override string GetTransformSummary() =>
        $"Remove query parameter '{QueryRemoveParameter}'";
}
