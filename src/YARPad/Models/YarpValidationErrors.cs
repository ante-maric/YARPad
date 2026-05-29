namespace CodingCell.YARPad;

public record YarpValidationErrors(YarpConfigurationSection Section, string EntityID, List<string> Errors);
