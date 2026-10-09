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
    public Task<string?> CreateAsync(Guid configurationProfileID)
    {
        return OpenEditorAsync(configurationProfileID, new ClusterModel() { ClusterID = "" });
    }

    public async Task<string?> OpenAsync(Guid configurationProfileID, string clusterID, bool validateWhenOpened = false)
    {
        var cluster = FindCluster(configurationProfileID, clusterID);
        if (cluster == null)
            return null;

        return await OpenEditorAsync(configurationProfileID, cluster.DeepClone(), clusterID, validateWhenOpened);
    }

    public async Task<string?> CloneAsync(Guid configurationProfileID, string clusterID)
    {
        var cluster = FindCluster(configurationProfileID, clusterID);
        if (cluster == null)
            return null;

        var clonedCluster = cluster.DeepClone();
        clonedCluster.ClusterID += " Cloned";

        return await OpenEditorAsync(configurationProfileID, clonedCluster);
    }

    private ClusterModel? FindCluster(Guid configurationProfileID, string clusterID)
    {
        return stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID)?.Configuration.Clusters.FirstOrDefault(x => x.ClusterID == clusterID);
    }

    // cluster is a draft the editor may change; never an object from the store.
    private async Task<string?> OpenEditorAsync(Guid configurationProfileID, ClusterModel cluster, string? clusterID = null, bool validateWhenOpened = false)
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
            { x => x.ConfigurationProfileID, profile.ID },
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
            await configurationProvider.SaveClusterAsync(profile.ID, clusterID, savedCluster);
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
