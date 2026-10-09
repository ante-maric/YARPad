using MudBlazor;
using Microsoft.Extensions.Logging;
using CodingCell.YARPad.Components.CustomTransforms;

namespace CodingCell.YARPad;

internal class CustomTransformEditorService(
    IDialogService dialogService,
    IYARPadConfigurationProvider configurationProvider,
    IStateStore<ConfigurationProfileState> stateStore,
    ILogger<CustomTransformEditorService> logger) : ICustomTransformEditorService
{
    public Task<bool> CreateAsync(Guid configurationProfileID)
    {
        return OpenEditorAsync(configurationProfileID, new CustomTransformDefinition() { Type = "" });
    }

    public async Task<bool> OpenAsync(Guid configurationProfileID, string transformType)
    {
        var transform = FindTransform(configurationProfileID, transformType);
        if (transform == null)
            return false;

        return await OpenEditorAsync(configurationProfileID, transform.DeepClone(), transformType);
    }

    public async Task<bool> CloneAsync(Guid configurationProfileID, string transformType)
    {
        var transform = FindTransform(configurationProfileID, transformType);
        if (transform == null)
            return false;

        var clonedTransform = transform.DeepClone();
        clonedTransform.Type += " Cloned";

        return await OpenEditorAsync(configurationProfileID, clonedTransform);
    }

    private CustomTransformDefinition? FindTransform(Guid configurationProfileID, string transformType)
    {
        return stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID)?.Configuration.CustomTransforms.FirstOrDefault(x => x.Type == transformType);
    }

    // transform is a draft the editor may change; never an object from the store.
    private async Task<bool> OpenEditorAsync(Guid configurationProfileID, CustomTransformDefinition transform, string? originalType = null)
    {
        var profile = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID);
        if (profile == null)
        {
            logger.LogWarning("Configuration profile {ConfigurationProfileID} not found for custom transform editor", configurationProfileID);
            return false;
        }

        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
        };

        var parameters = new DialogParameters<CustomTransformDialog>()
        {
            { x => x.Transform, transform },
            { x => x.OriginalType, originalType },
            { x => x.ConfigurationProfileID, profile.ID },
        };

        var dialog = await dialogService.ShowAsync<CustomTransformDialog>(null, parameters, options);
        var result = await dialog.Result;

        if (result == null || result.Canceled || result.Data is not CustomTransformDefinition savedTransform)
        {
            logger.LogDebug("Custom transform dialog was canceled or returned no data");
            return false;
        }

        try
        {
            await configurationProvider.SaveCustomTransformAsync(profile.ID, originalType, savedTransform);
            logger.LogInformation("Saved custom transform {TransformType} to configuration profile {ConfigurationProfileID}", savedTransform.Type, profile.ID);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save custom transform {TransformType} to configuration profile {ConfigurationProfileID}", originalType ?? "<new>", profile.ID);
            throw;
        }
    }
}
