namespace CodingCell.YARPad;

internal interface IRouteEditorService
{
    Task<bool> CreateAsync(Guid configurationProfileID);
    Task<bool> OpenAsync(Guid configurationProfileID, string routeID, bool validateWhenOpened = false);
    Task<bool> CloneAsync(Guid configurationProfileID, string routeID);
}
