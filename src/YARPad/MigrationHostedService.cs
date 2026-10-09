using Microsoft.Extensions.Hosting;

namespace CodingCell.YARPad;

internal sealed class MigrationHostedService(DatabaseMigrationGate migrationGate) : IHostedService
{
    // Migrate eagerly at startup so a failing migration stops the app before it serves requests.
    public Task StartAsync(CancellationToken cancellationToken) => migrationGate.WaitAsync();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
