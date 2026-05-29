using MudBlazor;
using Microsoft.Extensions.Logging;
using CodingCell.YARPad.Components.Route;

namespace CodingCell.YARPad;

internal class RouteEditorService(
    IDialogService dialogService, 
    IYARPadConfigurationProvider configurationProvider,
    IStateStore<ConfigurationProfileState> stateStore,
    ILogger<RouteEditorService> logger) : IRouteEditorService
{
    public async Task<bool> OpenAsync(Guid configurationProfileID, string routeID, bool validateWhenOpened = false)
    {
        var configuration = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID)?.Configuration;
        if (configuration == null)
            return false;

        var route = configuration.Routes.FirstOrDefault(x => x.RouteID == routeID);
        if (route == null)
            return false;

        return await OpenAsync(configurationProfileID, route.DeepClone(), routeID, validateWhenOpened);
    }

    public async Task<bool> OpenAsync(Guid configurationProfileID, RouteModel route, string? routeID = null, bool validateWhenOpened = false)
    {
        var profile = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID);
        if (profile == null)
        {
            logger.LogWarning("Configuration profile {ConfigurationProfileID} not found for route editor", configurationProfileID);
            return false;
        }

        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
        };

        var parameters = new DialogParameters<RouteDialog>()
        {
            { x => x.RouteID, routeID },
            { x => x.Route, route },
            { x => x.ConfigurationProfile, profile },
            { x => x.ValidateWhenOpened, validateWhenOpened }
        };

        var dialog = await dialogService.ShowAsync<RouteDialog>(null, parameters, options);
        var result = await dialog.Result;

        if (result == null || result.Canceled || result.Data is not RouteDialogResult dialogResult)
        {
            logger.LogDebug("Route dialog was canceled or returned no data");
            return false;
        }

        try
        {
            await configurationProvider.SaveRouteAsync(profile.ID, profile.Configuration, routeID, dialogResult.Route, dialogResult.BeforeRouteID);
            logger.LogInformation("Saved route {RouteID} to configuration profile {ConfigurationProfileID}", dialogResult.Route.RouteID, profile.ID);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save route {RouteID} to configuration profile {ConfigurationProfileID}", routeID ?? "<new>", profile.ID);
            throw;
        }
    }
}
