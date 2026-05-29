using System.Text.Json;
using CodingCell.YARPad.Data;

namespace CodingCell.YARPad;

internal static class YARPadConfigurationEntityExtensions
{
    extension(YARPadConfigurationEntity entity)
    {
        public ConfigurationProfile ToConfigurationProfile()
        {
            return new()
            {
                ID = entity.ID,
                Name = entity.Name,
                Description = entity.Description,
                Configuration = JsonSerializer.Deserialize<YARPadConfiguration>(entity.ConfigurationJson)!,
                IsActive = entity.IsActive
            };
        }
    }
}
