using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodingCell.YARPad;

/// <summary>
/// Hosted service that manages the lifecycle of the configuration change notifier.
/// </summary>
internal sealed class ConfigChangeNotifierHostedService : IHostedService
{
    private readonly IConfigChangeNotifier _notifier;
    private readonly ILogger<ConfigChangeNotifierHostedService> _logger;

    public ConfigChangeNotifierHostedService(
        IConfigChangeNotifier notifier,
        ILogger<ConfigChangeNotifierHostedService> logger)
    {
        _notifier = notifier;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting configuration change notifier");
        await _notifier.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping configuration change notifier");
        await _notifier.StopAsync(cancellationToken);
    }
}
