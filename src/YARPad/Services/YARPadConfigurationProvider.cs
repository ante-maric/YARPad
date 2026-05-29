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
    IMapper mapper,
    IOptions<YARPadOptions> yarpadOptions,
    ILogger<YARPadConfigurationProvider> logger) : IYARPadConfigurationProvider
{
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

    private Task SaveAsync(Guid configurationID, YARPadConfiguration configuration)
    {
        return ExecuteDbActionAsync(async context =>
        {
            var configurationJson = JsonSerializer.Serialize(configuration);

            var configurationEntity = await context.YARPadConfigurations.FirstOrDefaultAsync(x => x.ID == configurationID)
                ?? throw new InvalidOperationException("Configuration not found");

            configurationEntity.ConfigurationJson = configurationJson;
            configurationEntity.UpdatedOn = DateTime.UtcNow;
            configurationEntity.Version++;
            configurationEntity.LastModifiedByInstanceID = _instanceID;

            context.Update(configurationEntity);
            await context.SaveChangesAsync();

            logger.LogDebug("Saved configuration profile {ConfigurationID} (version {Version})", configurationID, configurationEntity.Version);

            if (configurationEntity.IsActive)
            {
                logger.LogDebug("Triggering active configuration changed event for {ConfigurationID}", configurationID);
                ActiveConfigurationChanged?.Invoke(configurationEntity);
            }
        });
    }

    public async Task SaveClusterAsync(Guid configurationID, YARPadConfiguration configuration, string? clusterID, ClusterModel clusterModel)
    {
        if (clusterID == null)
            configuration.Clusters.Add(clusterModel);
        else
        {
            var oldCluster = configuration.Clusters.Find(x => x.ClusterID == clusterID) ?? new ClusterModel() { ClusterID = clusterModel.ClusterID };

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

        await SaveAsync(configurationID, configuration);
    }

    public async Task SaveRouteAsync(Guid configurationID, YARPadConfiguration configuration, string? routeID, RouteModel routeModel, string? beforeRouteId)
    {
        var configurationJson = JsonSerializer.Serialize(configuration);

        RouteModel routeToUpdate;
        if (routeID == null)
        {
            configuration.Routes.Add(routeModel);
            routeToUpdate = routeModel;
        }
        else
        {
            routeToUpdate = configuration.Routes.Find(x => x.RouteID == routeID)!;
            mapper.Map(routeModel, routeToUpdate);
        }

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

        await SaveAsync(configurationID, configuration);
    }

    public async Task DeleteRouteAsync(Guid configurationID, YARPadConfiguration configuration, RouteModel routeModel)
    {
        configuration.Routes.Remove(routeModel);

        await SaveAsync(configurationID, configuration);
    }

    public async Task ToggleRouteAsync(Guid configurationID, YARPadConfiguration configuration, string routeID, bool isEnabled)
    {
        configuration.Routes.Find(x => x.RouteID == routeID)?.IsEnabled = isEnabled;

        await SaveAsync(configurationID, configuration);
    }

    public async Task DeleteClusterAsync(Guid configurationID, YARPadConfiguration configuration, ClusterModel clusterModel)
    {
        configuration.Clusters.Remove(clusterModel);

        await SaveAsync(configurationID, configuration);
    }

    public async Task SaveCustomTransformAsync(Guid configurationID, YARPadConfiguration configuration, string? originalType, CustomTransformDefinition definition)
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

        await SaveAsync(configurationID, configuration);
    }

    public async Task DeleteCustomTransformAsync(Guid configurationID, YARPadConfiguration configuration, CustomTransformDefinition definition)
    {
        configuration.CustomTransforms.Remove(definition);

        await SaveAsync(configurationID, configuration);
    }

    public async Task SavePolicyAsync(Guid configurationID, YARPadConfiguration configuration, string? policyID, PolicyInfo policy, PolicyType policyType)
    {
        var policies = configuration.Policies[policyType];

        if (policyID == null)
            policies.Add(policy);
        else
        {
            var oldPolicy = policies.Find(x => x.ID == policyID) ?? new PolicyInfo() { ID = policy.ID, Name = policy.ID.HumanizeTitle() };
            mapper.Map(policy, oldPolicy);
        }

        await SaveAsync(configurationID, configuration);
    }

    public async Task DeletePolicyAsync(Guid configurationID, YARPadConfiguration configuration, PolicyInfo policy, PolicyType policyType)
    {
        var policies = configuration.Policies[policyType];

        policies.Remove(policy);

        await SaveAsync(configurationID, configuration);
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
        using var scope = serviceScopeFactory.CreateScope();
        dbContext ??= scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await action(dbContext);
    }

    private async Task<T> ExecuteDbActionAsync<T>(Func<ApplicationDbContext, Task<T>> action, ApplicationDbContext? dbContext = null)
    {
        using var scope = serviceScopeFactory.CreateScope();
        dbContext ??= scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await action(dbContext);
    }
}
