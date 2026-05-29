namespace CodingCell.YARPad;

public sealed record CustomTransform : RouteTransform
{
    public override string TransformType => CustomTransformType;

    public string CustomTransformType { get; set; } = default!;
    public override string Name => CustomTransformType.HumanizeTitle();
    public override string Description { get; set; } = string.Empty;

    public List<CustomTransformParameter> Parameters { get; set; } = [];

    public List<CustomTransformParameterDefinition> ParameterDefinitions = [];

    public override RouteTransformGroup GetGroup() => RouteTransformGroup.Other;

    public override string GetTransformSummary()
    {
        var formattedParameters = Parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{p.Key}: {p.Value}")
            .ToList();

        if (formattedParameters.Count == 0)
            return "No parameters";

        return string.Join(", ", formattedParameters);
    }
}
