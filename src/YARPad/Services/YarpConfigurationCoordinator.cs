using CodingCell.YARPad.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

internal class YarpConfigurationCoordinator : IDisposable
{
    private readonly IYARPadConfigurationProvider _yarpadConfigurationProvider;
    private readonly IStoreWriter<YarpConfigStatusState> _stateStore;
    private readonly IStoreWriter<ConfigurationProfileState> _configurationProfileStateStore;
    private readonly IYarpConfigProvider _yarpConfigProvider;
    private readonly IConfigChangeNotifier _configChangeNotifier;
    private readonly ILogger<YarpConfigurationCoordinator> _logger;

    public YarpConfigurationCoordinator(
        IYARPadConfigurationProvider yarpadConfigurationProvider, 
        IStoreWriter<YarpConfigStatusState> stateStore,
        IStoreWriter<ConfigurationProfileState> configurationProfileStateStore,
        IYarpConfigProvider yarpConfigProvider,
        IConfigChangeNotifier configChangeNotifier,
        IOptions<YARPadOptions> yarpadOptions,
        ILogger<YarpConfigurationCoordinator> logger)
    {
        _yarpadConfigurationProvider = yarpadConfigurationProvider;
        _stateStore = stateStore;
        _configurationProfileStateStore = configurationProfileStateStore;
        _yarpConfigProvider = yarpConfigProvider;
        _configChangeNotifier = configChangeNotifier;
        _logger = logger;
    }

    public void Initialize()
    {
        _yarpadConfigurationProvider.ActiveConfigurationChanged += OnActiveConfigurationChanged;
        _yarpadConfigurationProvider.ConfigurationProfilesChanged += OnConfigurationProfilesChanged;
        _yarpConfigProvider.ConfigStatusChanged += OnYarpConfigStatusChanged;
        if (_configChangeNotifier.IsEnabled)
            _configChangeNotifier.ConfigurationChanged += OnRemoteConfigurationChangedAsync;
    }

    private void OnConfigurationProfilesChanged(List<YARPadConfigurationEntity> configurations)
    {
        _configurationProfileStateStore.Update(x => configurations.ToState());
    }

    private async void OnYarpConfigStatusChanged(Guid configurationID, YARPadConfigurationStatus status, List<YarpValidationErrors>? errors = null)
    {
        _logger.LogInformation("YARP configuration status changed for {ConfigurationID}: {Status}", configurationID, status);

        if (errors?.Count > 0)
            _logger.LogWarning("YARP configuration {ConfigurationID} has {ErrorCount} validation errors", configurationID, errors.Count);

        _stateStore.Update(x => new(configurationID, status, errors ?? [], DateTime.UtcNow));
        if (status == YARPadConfigurationStatus.Applied)
            await _yarpadConfigurationProvider.ActivateConfigurationAsync(configurationID);
    }

    private async void OnActiveConfigurationChanged(YARPadConfigurationEntity configurationEntity)
    {
        _logger.LogInformation("Active configuration changed to {ConfigurationID} ('{Name}')", configurationEntity.ID, configurationEntity.Name);

        try
        {
            await _yarpConfigProvider.UpdateConfigurationAsync(configurationEntity.ToConfigurationProfile());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update YARP config provider with configuration {ConfigurationID}", configurationEntity.ID);
        }
    }

    /// <summary>
    /// Handles configuration change notifications from other YARP instances.
    /// </summary>
    private async Task OnRemoteConfigurationChangedAsync(ConfigChangeNotification notification)
    {
        _logger.LogInformation(
            "Received remote configuration change notification for {ConfigurationID} (version {Version})",
            notification.ConfigurationId, notification.Version);

        try
        {
            // Trigger a reload of the active configuration from the database
            await _yarpadConfigurationProvider.RequestConfigurationActivationAsync(notification.ConfigurationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to reload configuration {ConfigurationID} after remote change notification", 
                notification.ConfigurationId);
        }
    }

    public void Dispose()
    {
        _yarpadConfigurationProvider.ActiveConfigurationChanged -= OnActiveConfigurationChanged;
        _yarpadConfigurationProvider.ConfigurationProfilesChanged -= OnConfigurationProfilesChanged;
        _yarpConfigProvider.ConfigStatusChanged -= OnYarpConfigStatusChanged;
        _configChangeNotifier.ConfigurationChanged -= OnRemoteConfigurationChangedAsync;
    }
}
