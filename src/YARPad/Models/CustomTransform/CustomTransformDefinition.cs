using System.Text.Json;

namespace CodingCell.YARPad;

public record CustomTransformDefinition
{
    public required string Type { get; set; }

    public string? Description { get; set; }

    public List<CustomTransformParameterDefinition> Parameters { get; set; } = [];

    public CustomTransformDefinition DeepClone()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<CustomTransformDefinition>(json)!;
    }
}
