namespace CodingCell.YARPad;

public class ConfigurationProfile
{
    public Guid ID { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required YARPadConfiguration Configuration { get; set; }
    public bool IsActive { get; set; }
}