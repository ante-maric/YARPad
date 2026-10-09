using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodingCell.YARPad.Data;

namespace CodingCell.YARPad;

internal class YARPadConfigurationProvider(
    IServiceScopeFactory serviceScopeFactory,
    DatabaseMigrationGate migrationGate,
    IMapper mapper,
    IOptions<YARPadOptions> yarpadOptions,
    ILogger<YARPadConfigurationProvider> logger) : IYARPadConfigurationProvider
{
    private const int MaxSaveAttempts = 5;

    private readonly string _instanceID = yarpadOptions.Value.InstanceID;

    public event Action<YARPadConfigurationEntity>? ActiveConfigurationChanged;
    public event Action<List<YARPadConfigurationEntity>>? ConfigurationProfilesChanged;

    public Task<YARPadConfigurationEntity> CreateConfigurationAsync(string name, string? description)
    {
        logger.LogInformation("Creating new configuration profile '{Name}'", name);

        return AddConfigurationAsync(CreateConfigurationEntity(name, description));
    }

    public Task<YARPadConfigurationEntity> ImportConfigurationAsync(ConfigurationProfile configurationProfile)
    {
        logger.LogInformation("Importing configuration profile '{Name}'", configurationProfile.Name);

        var entity = CreateConfigurationEntity(
            configurationProfile.Name,
            configurationProfile.Description,
            JsonSerializer.Serialize(configurationProfile.Configuration));

        return AddConfigurationAsync(entity);
    }

    public async Task<YARPadConfigurationEntity> CloneConfigurationAsync(Guid configurationID, string name, string? description)
    {
        logger.LogInformation("Cloning configuration profile {ConfigurationID} to '{Name}'", configurationID, name);

        var sourceConfiguration = await ExecuteDbActionAsync(context => context.YARPadConfigurations.FirstOrDefaultAsync(x => x.ID == configurationID));

        if (sourceConfiguration == null)
            logger.LogWarning("Source configuration {ConfigurationID} not found for cloning", configurationID);

        var cloned = CreateConfigurationEntity(name, description, sourceConfiguration?.ConfigurationJson);

        return await AddConfigurationAsync(cloned);
    }

    public async Task<List<YARPadConfigurationEntity>> GetConfigurationsAsync()
    {
        return await ExecuteDbActionAsync(async context =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                List<YARPadConfigurationEntity> configurations;

                if (!(await context.YARPadConfigurations.AnyAsync()))
                {
                    var entity = CreateConfigurationEntity("Main", "Main configuration that is created by default");
                    entity.IsActive = true;
                    context.Add(entity);
                    await context.SaveChangesAsync();
                    configurations = [entity];
                }
                else
                    configurations = await context.YARPadConfigurations.ToListAsync();

                await transaction.CommitAsync();

                return configurations;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task RequestConfigurationActivationAsync(Guid configurationID)
    {
        logger.LogInformation("Requesting activation of configuration profile {ConfigurationID}", configurationID);

        var configuration = await ExecuteDbActionAsync(context => context.YARPadConfigurations.FirstOrDefaultAsync(x => x.ID == configurationID));
        if (configuration != null)
            ActiveConfigurationChanged?.Invoke(configuration);
        else
            logger.LogWarning("Configuration {ConfigurationID} not found for activation request", configurationID);
    }

    public async Task ActivateConfigurationAsync(Guid configurationID)
    {
        logger.LogInformation("Activating configuration profile {ConfigurationID}", configurationID);

        try
        {
            await ExecuteDbActionAsync(async context =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    await context.YARPadConfigurations
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, p => p.ID == configurationID));

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            var configurations = await GetConfigurationsAsync();
            ConfigurationProfilesChanged?.Invoke(configurations);

            logger.LogInformation("Successfully activated configuration profile {ConfigurationID}", configurationID);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to activate configuration profile {ConfigurationID}", configurationID);
            throw;
        }
    }

    public async Task DeleteConfigurationAsync(Guid configurationID)
    {
        logger.LogInformation("Deleting configuration profile {ConfigurationID}", configurationID);

        try
        {
            await ExecuteDbActionAsync(async context =>
            {
                await context.YARPadConfigurations.Where(x => x.ID == configurationID && !x.IsActive).ExecuteDeleteAsync();
            });

            var configurations = await GetConfigurationsAsync();
            ConfigurationProfilesChanged?.Invoke(configurations);

            logger.LogInformation("Successfully deleted configuration profile {ConfigurationID}", configurationID);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete configuration profile {ConfigurationID}", configurationID);
            throw;
        }
    }

    private async Task<YARPadConfigurationEntity> AddConfigurationAsync(YARPadConfigurationEntity? entity = null)
    {
        var configuration = await ExecuteDbActionAsync(async context =>
        {
            if (entity == null)
                entity = CreateConfigurationEntity("Main", "Main configuration that is created by default");
            else
                entity.UpdatedOn = DateTime.UtcNow;

            context.Add(entity);
            await context.SaveChangesAsync();

            return entity;
        });

        var configurations = await GetConfigurationsAsync();
        ConfigurationProfilesChanged?.Invoke(configurations);

        return configurations.First(x => x.ID == configuration.ID);
    }

    public async Task UpdateConfigurationAsync(Guid configurationID, string name, string? description)
    {
        await ExecuteDbActionAsync(async context =>
        {
            await context.YARPadConfigurations
                .Where(x => x.ID == configurationID)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(p => p.Name, _ => name)
                    .SetProperty(p => p.Description, _ => description)
                    .SetProperty(p => p.UpdatedOn, _ => DateTime.UtcNow));
        });

        var configurations = await GetConfigurationsAsync();
        ConfigurationProfilesChanged?.Invoke(configurations);
    }

    /// <summary>
    /// Loads the stored configuration, applies <paramref name="modify"/> to it and saves it.
    /// </summary>
    /// <remarks>
    /// The configuration is loaded at commit time instead of being passed in by the caller,
    /// so changes saved since the caller read its state are not overwritten.
    /// <para>
    /// The save only succeeds if the stored version is still the loaded one. If another save (from this or
    /// another instance) got in between, the configuration is reloaded and <paramref name="modify"/> is applied
    /// again, so <paramref name="modify"/> must only change the configuration it is given.
    /// </para>
    /// </remarks>
    private async Task ModifyConfigurationAsync(Guid configurationID, Action<YARPadConfiguration> modify)
    {
        await ExecuteDbActionAsync(async context =>
        {
            YARPadConfigurationEntity configurationEntity;

            for (var attempt = 1; ; attempt++)
            {
                configurationEntity = await context.YARPadConfigurations.AsNoTracking().FirstOrDefaultAsync(x => x.ID == configurationID)
                    ?? throw new InvalidOperationException("Configuration not found");

                var configuration = JsonSerializer.Deserialize<YARPadConfiguration>(configurationEntity.ConfigurationJson)!;
                modify(configuration);

                var loadedVersion = configurationEntity.Version;
                configurationEntity.ConfigurationJson = JsonSerializer.Serialize(configuration);
                configurationEntity.UpdatedOn = DateTime.UtcNow;
                configurationEntity.Version = loadedVersion + 1;
                configurationEntity.LastModifiedByInstanceID = _instanceID;

                var updatedCount = await context.YARPadConfigurations
                    .Where(x => x.ID == configurationID && x.Version == loadedVersion)
                    .ExecuteUpdateAsync(x => x
                        .SetProperty(p => p.ConfigurationJson, configurationEntity.ConfigurationJson)
                        .SetProperty(p => p.UpdatedOn, configurationEntity.UpdatedOn)
                        .SetProperty(p => p.Version, configurationEntity.Version)
                        .SetProperty(p => p.LastModifiedByInstanceID, configurationEntity.LastModifiedByInstanceID));

                if (updatedCount == 1)
                    break;

                if (attempt == MaxSaveAttempts)
                    throw new DbUpdateConcurrencyException($"Configuration {configurationID} kept being changed by other saves; gave up after {MaxSaveAttempts} attempts.");

                logger.LogDebug("Configuration profile {ConfigurationID} was saved by someone else since version {Version} was loaded, retrying", configurationID, loadedVersion);
            }

            logger.LogDebug("Saved configuration profile {ConfigurationID} (version {Version})", configurationID, configurationEntity.Version);

            if (configurationEntity.IsActive)
            {
                logger.LogDebug("Triggering active configuration changed event for {ConfigurationID}", configurationID);
                ActiveConfigurationChanged?.Invoke(configurationEntity);
            }
        });

        var configurations = await GetConfigurationsAsync();
        ConfigurationProfilesChanged?.Invoke(configurations);
    }

    public Task SaveClusterAsync(Guid configurationID, string? clusterID, ClusterModel clusterModel)
    {
        return ModifyConfigurationAsync(configurationID, configuration =>
        {
            // A cluster deleted since the editor was opened is saved as a new one.
            var oldCluster = clusterID != null ? configuration.Clusters.Find(x => x.ClusterID == clusterID) : null;
            if (oldCluster == null)
                configuration.Clusters.Add(clusterModel);
            else
            {
                var oldClusterID = oldCluster.ClusterID;
                mapper.Map(clusterModel, oldCluster);
                var newClusterID = oldCluster.ClusterID;

                if (oldClusterID != newClusterID)
                {
                    logger.LogInformation("Cluster renamed from '{OldClusterID}' to '{NewClusterID}', updating route references", oldClusterID, newClusterID);

                    foreach (var route in configuration.Routes.Where(r => r.ClusterID == oldClusterID))
                        route.ClusterID = newClusterID;
                }
            }
        });
    }

    public Task SaveRouteAsync(Guid configurationID, string? routeID, RouteModel routeModel, string? beforeRouteId)
    {
        return ModifyConfigurationAsync(configurationID, configuration =>
        {
            // A route deleted since the editor was opened is saved as a new one.
            var routeToUpdate = routeID != null ? configuration.Routes.Find(x => x.RouteID == routeID) : null;
            if (routeToUpdate == null)
            {
                configuration.Routes.Add(routeModel);
                routeToUpdate = routeModel;
            }
            else
                mapper.Map(routeModel, routeToUpdate);

            // Move the route to the correct position
            configuration.Routes.Remove(routeToUpdate);

            var targetIndex = configuration.Routes.Count;
            if (!string.IsNullOrEmpty(beforeRouteId))
            {
                var beforeRouteIndex = configuration.Routes.FindIndex(r => r.RouteID == beforeRouteId);
                if (beforeRouteIndex != -1)
                    targetIndex = beforeRouteIndex;
            }
            configuration.Routes.Insert(targetIndex, routeToUpdate);
        });
    }

    public Task DeleteRouteAsync(Guid configurationID, RouteModel routeModel)
    {
        return ModifyConfigurationAsync(configurationID, configuration => configuration.Routes.RemoveAll(x => x.RouteID == routeModel.RouteID));
    }

    public Task ToggleRouteAsync(Guid configurationID, string routeID, bool isEnabled)
    {
        return ModifyConfigurationAsync(configurationID, configuration => configuration.Routes.Find(x => x.RouteID == routeID)?.IsEnabled = isEnabled);
    }

    public Task DeleteClusterAsync(Guid configurationID, ClusterModel clusterModel)
    {
        return ModifyConfigurationAsync(configurationID, configuration => configuration.Clusters.RemoveAll(x => x.ClusterID == clusterModel.ClusterID));
    }

    public Task SaveCustomTransformAsync(Guid configurationID, string? originalType, CustomTransformDefinition definition)
    {
        return ModifyConfigurationAsync(configurationID, configuration =>
        {
            if (originalType == null)
                configuration.CustomTransforms.Add(definition);
            else
            {
                var existingIndex = configuration.CustomTransforms.FindIndex(x => x.Type == originalType);
                if (existingIndex >= 0)
                    configuration.CustomTransforms[existingIndex] = definition;
                else
                    configuration.CustomTransforms.Add(definition);
            }
        });
    }

    public Task DeleteCustomTransformAsync(Guid configurationID, CustomTransformDefinition definition)
    {
        return ModifyConfigurationAsync(configurationID, configuration => configuration.CustomTransforms.RemoveAll(x => x.Type == definition.Type));
    }

    public Task SavePolicyAsync(Guid configurationID, string? policyID, PolicyInfo policy, PolicyType policyType)
    {
        return ModifyConfigurationAsync(configurationID, configuration =>
        {
            var policies = configuration.Policies[policyType];

            // A policy deleted since the editor was opened is saved as a new one.
            var oldPolicy = policyID != null ? policies.Find(x => x.ID == policyID) : null;
            if (oldPolicy == null)
                policies.Add(policy);
            else
                mapper.Map(policy, oldPolicy);
        });
    }

    public Task DeletePolicyAsync(Guid configurationID, PolicyInfo policy, PolicyType policyType)
    {
        return ModifyConfigurationAsync(configurationID, configuration => configuration.Policies[policyType].RemoveAll(x => x.ID == policy.ID));
    }

    private static YARPadConfiguration CreateConfiguration()
    {
        var configuration = new YARPadConfiguration();

        foreach (var policyType in Enum.GetValues<PolicyType>().Except(configuration.Policies.Keys).ToArray())
            configuration.Policies[policyType] = [];

        return configuration;
    }

    private static YARPadConfigurationEntity CreateConfigurationEntity(string name, string? description, string? configurationJson = null)
    {
        return new()
        {
            ID = Guid.NewGuid(),
            Name = name,
            Description = description,
            ConfigurationJson = configurationJson ?? JsonSerializer.Serialize(CreateConfiguration()),
            IsActive = false,
            CreatedOn = DateTime.UtcNow
        };
    }

    private async Task ExecuteDbActionAsync(Func<ApplicationDbContext, Task> action, ApplicationDbContext? dbContext = null)
    {
        await migrationGate.WaitAsync();

        using var scope = serviceScopeFactory.CreateScope();
        dbContext ??= scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(dbContext);
    }

    private async Task<T> ExecuteDbActionAsync<T>(Func<ApplicationDbContext, Task<T>> action, ApplicationDbContext? dbContext = null)
    {
        await migrationGate.WaitAsync();

        using var scope = serviceScopeFactory.CreateScope();
        dbContext ??= scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await action(dbContext);
    }
}
