using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodingCell.YARPad;

internal sealed class MigrationHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
        await migrationService.ApplyMigrationsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
