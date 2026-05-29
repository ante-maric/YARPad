namespace CodingCell.YARPad;

internal interface IClusterEditorService
{
    Task<string?> OpenAsync(Guid configurationProfileID, string clusterID, bool validateWhenOpened = false);
    Task<string?> OpenAsync(Guid configurationProfileID, ClusterModel cluster, string? clusterID = null, bool validateWhenOpened = false);
}
