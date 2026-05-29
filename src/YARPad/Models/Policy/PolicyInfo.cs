namespace CodingCell.YARPad;

public record PolicyInfo
{
    public required string ID { get; set; }
    public required string Name { get; set; }
    public bool IsBuiltIn { get; set; }
    public string? Description { get; set; }
}
