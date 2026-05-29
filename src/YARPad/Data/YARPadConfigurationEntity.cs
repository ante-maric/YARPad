using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCell.YARPad.Data;

public class YARPadConfigurationEntity
{
    [Key]
    public Guid ID { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "TEXT")]
    public required string ConfigurationJson { get; set; }

    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long Version { get; set; }

    [MaxLength(50)]
    public string? LastModifiedByInstanceID { get; set; }
}
