namespace CodingCell.YARPad;

internal interface IClusterEditorService
{
    Task<string?> CreateAsync(Guid configurationProfileID);
    Task<string?> OpenAsync(Guid configurationProfileID, string clusterID, bool validateWhenOpened = false);
    Task<string?> CloneAsync(Guid configurationProfileID, string clusterID);
}
