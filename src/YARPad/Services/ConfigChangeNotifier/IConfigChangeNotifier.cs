namespace CodingCell.YARPad;

/// <summary>
/// Abstraction for notifying multiple YARP instances about configuration changes.
/// Implementations can use different mechanisms (database polling, Redis pub/sub, message queues, etc.).
/// </summary>
public interface IConfigChangeNotifier : IAsyncDisposable
{
    /// <summary>
    /// Event raised when a configuration change is detected from another instance.
    /// </summary>
    event Func<ConfigChangeNotification, Task>? ConfigurationChanged;

    bool IsEnabled { get; }

    /// <summary>
    /// Publishes a notification that the configuration has changed.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration that changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task NotifyChangeAsync(Guid configurationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts listening for configuration changes from other instances.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops listening for configuration changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Notification payload for configuration changes.
/// </summary>
/// <param name="ConfigurationId">The ID of the configuration that changed.</param>
/// <param name="Version">The new version of the configuration.</param>
/// <param name="Timestamp">When the change occurred.</param>
public record ConfigChangeNotification(Guid ConfigurationId, long Version, DateTime Timestamp);
