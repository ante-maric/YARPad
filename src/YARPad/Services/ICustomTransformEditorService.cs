namespace CodingCell.YARPad;

internal interface ICustomTransformEditorService
{
    Task<bool> CreateAsync(Guid configurationProfileID);
    Task<bool> OpenAsync(Guid configurationProfileID, string transformType);
    Task<bool> CloneAsync(Guid configurationProfileID, string transformType);
}
