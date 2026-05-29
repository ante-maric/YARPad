using MudBlazor;
using Microsoft.Extensions.Logging;
using CodingCell.YARPad.Components.Cluster;

namespace CodingCell.YARPad;

internal class ClusterEditorService(
    IDialogService dialogService, 
    IYARPadConfigurationProvider configurationProvider,
    IStateStore<ConfigurationProfileState> stateStore,
    ILogger<ClusterEditorService> logger) : IClusterEditorService
{
    public async Task<string?> OpenAsync(Guid configurationProfileID, string clusterID, bool validateWhenOpened = false)
    {
        var configuration = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID)?.Configuration;
        if (configuration == null)
            return null;

        var cluster = configuration.Clusters.FirstOrDefault(x => x.ClusterID == clusterID);
        if (cluster == null)
            return null;

        return await OpenAsync(configurationProfileID, cluster.DeepClone(), clusterID, validateWhenOpened);
    }

    public async Task<string?> OpenAsync(Guid configurationProfileID, ClusterModel cluster, string? clusterID = null, bool validateWhenOpened = false)
    {
        var profile = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID);
        if (profile == null)
        {
            logger.LogWarning("Configuration profile {ConfigurationProfileID} not found for cluster editor", configurationProfileID);
            return null;
        }

        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
        };

        var parameters = new DialogParameters<ClusterDialog>()
        {
            { x => x.ClusterID, clusterID },
            { x => x.Cluster, cluster },
            { x => x.ValidateWhenOpened, validateWhenOpened }
        };

        var dialog = await dialogService.ShowAsync<ClusterDialog>(null, parameters, options);
        var result = await dialog.Result;

        if (result == null || result.Canceled || result.Data is not ClusterModel savedCluster)
        {
            logger.LogDebug("Cluster dialog was canceled or returned no data");
            return null;
        }

        try
        {
            await configurationProvider.SaveClusterAsync(profile.ID, profile.Configuration, clusterID, savedCluster);
            logger.LogInformation("Saved cluster {ClusterID} to configuration profile {ConfigurationProfileID}", savedCluster.ClusterID, profile.ID);

            return savedCluster.ClusterID;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save cluster {ClusterID} to configuration profile {ConfigurationProfileID}", clusterID ?? "<new>", profile.ID);
            throw;
        }
    }
}
