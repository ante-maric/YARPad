namespace CodingCell.YARPad;

public sealed record YarpMetadata
{
    public required string Key { get; set; }
    public string? Value { get; set; }
}
