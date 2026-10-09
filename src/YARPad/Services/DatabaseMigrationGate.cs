using Microsoft.Extensions.DependencyInjection;

namespace CodingCell.YARPad;

/// <summary>
/// Applies database migrations once, on first use, so that database readers
/// (e.g. the YARP config provider, which is queried before hosted services start)
/// don't access tables that do not exist yet. Running the migrations here rather than
/// waiting for a hosted service to signal completion means hosts that never start
/// hosted services (tests, tools) can't wait forever.
/// </summary>
internal sealed class DatabaseMigrationGate(IServiceScopeFactory scopeFactory)
{
    private readonly Lock _lock = new();
    private Task? _migration;

    public Task WaitAsync()
    {
        lock (_lock)
        {
            // A failed attempt is retried by the next caller.
            if (_migration == null || _migration.IsFaulted || _migration.IsCanceled)
                _migration = MigrateAsync();

            return _migration;
        }
    }

    private async Task MigrateAsync()
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
        await migrationService.ApplyMigrationsAsync();
    }
}
