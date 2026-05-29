namespace CodingCell.YARPad;

internal interface IRouteEditorService
{
    Task<bool> OpenAsync(Guid configurationProfileID, string routeID, bool validateWhenOpened = false);
    Task<bool> OpenAsync(Guid configurationProfileID, RouteModel route, string? routeID = null, bool validateWhenOpened = false);
}
