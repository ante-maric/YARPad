using AutoMapper;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

internal interface IYarpConfigProvider : IProxyConfigProvider
{
    event YarpConfigStatusChangedHandler? ConfigStatusChanged;

    Task UpdateConfigurationAsync(ConfigurationProfile configurationProfile);
}

internal delegate void YarpConfigStatusChangedHandler(Guid configurationID, YARPadConfigurationStatus status, List<YarpValidationErrors>? errors = null);

internal class YarpConfigProvider : IYarpConfigProvider, IDisposable
{
    public event YarpConfigStatusChangedHandler? ConfigStatusChanged;

    private readonly IYARPadConfigurationProvider _yarpadConfigurationProvider;
    private readonly IMapper _mapper;
    private readonly IConfigValidator _configValidator;

    private readonly SemaphoreSlim _updateLock = new(1, 1);
    private int _initialLoadStarted;
    private YarpConfig? _currentConfig;
    private Guid? _currentConfigurationID;
    private ConfigurationProfile? _pendingConfiguration;
    private int _updateRunning;

    private readonly ILogger<YarpConfigProvider> _logger;

    private bool _disposed;

    public YarpConfigProvider(
        IYARPadConfigurationProvider yarpadConfigurationProvider,
        IMapper mapper,
        IConfigValidator configValidator,
        ILogger<YarpConfigProvider> logger)
    {
        _yarpadConfigurationProvider = yarpadConfigurationProvider;
        _mapper = mapper;
        _configValidator = configValidator;
        _logger = logger;
    }

    public IProxyConfig GetConfig()
    {
        var config = Volatile.Read(ref _currentConfig);
        if (config != null)
        {
            ConfigStatusChanged?.Invoke(_currentConfigurationID!.Value, YARPadConfigurationStatus.Applied);
            return config;
        }

        var newConfig = new YarpConfig();
        if (Interlocked.CompareExchange(ref _currentConfig, newConfig, null) == null)
        {
            if (Interlocked.Exchange(ref _initialLoadStarted, 1) == 0)
                _ = LoadConfigurationAsync();

            return newConfig;
        }

        return _currentConfig!;
    }

    public Task UpdateConfigurationAsync(ConfigurationProfile configurationProfile) => CoalesceUpdateAsync(configurationProfile);

    private async Task LoadConfigurationAsync()
    {
        try
        {
            var configurations = await _yarpadConfigurationProvider.GetConfigurationsAsync();
            var activeProfile = configurations?.FirstOrDefault(x => x.IsActive)?.ToConfigurationProfile();

            if (activeProfile != null)
                _ = CoalesceUpdateAsync(activeProfile);
        }
        finally
        {
            Interlocked.Exchange(ref _initialLoadStarted, 0);
        }
    }

    private async Task CoalesceUpdateAsync(ConfigurationProfile configurationProfile)
    {
        Volatile.Write(ref _pendingConfiguration, configurationProfile);

        if (Interlocked.Exchange(ref _updateRunning, 1) == 0)
            await RunUpdateLoopAsync();
    }

    private async Task RunUpdateLoopAsync()
    {
        try
        {
            while (true)
            {
                var configuration = Interlocked.Exchange(ref _pendingConfiguration, null);
                if (configuration == null)
                    break;

                await UpdateConfigAsync(configuration);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _updateRunning, 0);
            if (Volatile.Read(ref _pendingConfiguration) != null && Interlocked.Exchange(ref _updateRunning, 1) == 0)
            {
                _ = RunUpdateLoopAsync();
            }
        }
    }

    private async Task UpdateConfigAsync(ConfigurationProfile configurationProfile)
    {
        await _updateLock.WaitAsync();

        try
        {
            var isInitialLoad = _currentConfigurationID == null;
            ConfigStatusChanged?.Invoke(configurationProfile.ID, YARPadConfigurationStatus.Loading);

            YarpConfig config;

            try
            {
                config = _mapper.Map<YarpConfig>(configurationProfile.Configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map YARPad configuration to YARP config.");
                return;
            }

            var errors = await ValidateConfigAsync(config);
            if (errors.Count > 0)
            {
                ConfigStatusChanged?.Invoke(configurationProfile.ID, isInitialLoad ? YARPadConfigurationStatus.Invalid : YARPadConfigurationStatus.RevertedToPrevious, errors);
                return;
            }

            _currentConfigurationID = configurationProfile.ID;
            var previousConfig = Interlocked.Exchange(ref _currentConfig, config);
            previousConfig?.SignalChange();
            previousConfig?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update YARP config.");
        }
        finally
        {
            _updateLock.Release();
        }
    }

    private async Task<List<YarpValidationErrors>> ValidateConfigAsync(YarpConfig config)
    {
        var tasks = new List<Task<YarpValidationErrors?>>(config.Routes.Count + config.Clusters.Count);

        foreach (var route in config.Routes)
            tasks.Add(ValidateRouteAsync(route));

        foreach (var cluster in config.Clusters)
            tasks.Add(ValidateClusterAsync(cluster));

        var result = await Task.WhenAll(tasks);
        var errors = result
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();

        return errors;

        async Task<YarpValidationErrors?> ValidateRouteAsync(RouteConfig route)
        {
            IList<Exception> errors;

            try
            {
                errors = await _configValidator.ValidateRouteAsync(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate YARP route {RouteID}.", route.RouteId ?? "<null>");
                return new(YarpConfigurationSection.Route, route.RouteId!, ["Failed to validate route."]);
            }

            if (errors.Count <= 0)
                return null;

            foreach (var error in errors)
                _logger.LogWarning(error, "YARP route validation error for {RouteID}.", route.RouteId ?? "<null>");

            return new(YarpConfigurationSection.Route, route.RouteId!, errors.Select(e => e.Message).ToList());
        }

        async Task<YarpValidationErrors?> ValidateClusterAsync(ClusterConfig cluster)
        {
            IList<Exception> errors;
            try
            {
                errors = await _configValidator.ValidateClusterAsync(cluster);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate YARP cluster {ClusterID}.", cluster.ClusterId ?? "<null>");
                return new(YarpConfigurationSection.Cluster, cluster.ClusterId!, ["Failed to validate cluster."]);
            }

            if (errors.Count <= 0)
                return null;

            foreach (var error in errors)
                _logger.LogWarning(error, "YARP cluster validation error for {ClusterID}.", cluster.ClusterId ?? "<null>");

            return new(YarpConfigurationSection.Cluster, cluster.ClusterId!, errors.Select(e => e.Message).ToList());
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _currentConfig?.Dispose();
        _updateLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
