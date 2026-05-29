namespace CodingCell.YARPad;

public record YarpConfigStatusState(Guid ConfigurationProfileID, YARPadConfigurationStatus Status, List<YarpValidationErrors> Errors, DateTime ModifiedUTC);
