namespace CodingCell.YARPad;

internal class NoOpConfigChangeNotifier : IConfigChangeNotifier
{
    public event Func<ConfigChangeNotification, Task>? ConfigurationChanged;

    public bool IsEnabled => false;

    public Task NotifyChangeAsync(Guid configurationId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
