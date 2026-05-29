using CodingCell.YARPad.Data;

namespace CodingCell.YARPad;

public interface IYARPadConfigurationProvider
{
    event Action<YARPadConfigurationEntity>? ActiveConfigurationChanged;
    event Action<List<YARPadConfigurationEntity>>? ConfigurationProfilesChanged;

    Task<YARPadConfigurationEntity> CreateConfigurationAsync(string name, string? description);
    Task<YARPadConfigurationEntity> CloneConfigurationAsync(Guid configurationID, string name, string? description);
    Task<YARPadConfigurationEntity> ImportConfigurationAsync(ConfigurationProfile configurationProfile);
    Task<List<YARPadConfigurationEntity>> GetConfigurationsAsync();
    Task RequestConfigurationActivationAsync(Guid configurationID);
    Task ActivateConfigurationAsync(Guid configurationID);
    Task UpdateConfigurationAsync(Guid configurationID, string name, string? description);
    Task DeleteConfigurationAsync(Guid configurationID);

    Task SaveClusterAsync(Guid configurationID, YARPadConfiguration configuration, string? clusterID, ClusterModel clusterModel);
    Task DeleteClusterAsync(Guid configurationID, YARPadConfiguration configuration, ClusterModel clusterModel);
    Task SaveRouteAsync(Guid configurationID, YARPadConfiguration configuration, string? routeID, RouteModel routeModel, string? precedingRouteId);
    Task DeleteRouteAsync(Guid configurationID, YARPadConfiguration configuration, RouteModel routeModel);
    Task ToggleRouteAsync(Guid configurationID, YARPadConfiguration configuration, string routeID, bool isEnabled);
    Task SaveCustomTransformAsync(Guid configurationID, YARPadConfiguration configuration, string? originalType, CustomTransformDefinition definition);
    Task DeleteCustomTransformAsync(Guid configurationID, YARPadConfiguration configuration, CustomTransformDefinition definition);
    Task SavePolicyAsync(Guid configurationID, YARPadConfiguration configuration, string? policyID, PolicyInfo policy, PolicyType policyType);
    Task DeletePolicyAsync(Guid configurationID, YARPadConfiguration configuration, PolicyInfo policy, PolicyType policyType);
}
