using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class DatabaseMigrationGateTests
{
    private readonly Mock<IDatabaseMigrationService> _migrationService = new();
    private readonly DatabaseMigrationGate _gate;

    public DatabaseMigrationGateTests()
    {
        var services = new ServiceCollection()
            .AddScoped(_ => _migrationService.Object)
            .BuildServiceProvider();

        _gate = new DatabaseMigrationGate(services.GetRequiredService<IServiceScopeFactory>());
    }

    [Fact]
    public async Task WaitAsync_ShouldMigrateOnce_WhenCalledRepeatedly()
    {
        var migration = new TaskCompletionSource();
        _migrationService.Setup(x => x.ApplyMigrationsAsync()).Returns(migration.Task);

        var first = _gate.WaitAsync();
        var second = _gate.WaitAsync();
        migration.SetResult();
        await Task.WhenAll(first, second, _gate.WaitAsync());

        _migrationService.Verify(x => x.ApplyMigrationsAsync(), Times.Once);
    }

    [Fact]
    public async Task WaitAsync_ShouldRetryMigration_WhenPreviousAttemptFailed()
    {
        _migrationService
            .SetupSequence(x => x.ApplyMigrationsAsync())
            .ThrowsAsync(new InvalidOperationException("Migration failed."))
            .Returns(Task.CompletedTask);

        await Should.ThrowAsync<InvalidOperationException>(() => _gate.WaitAsync());
        await _gate.WaitAsync();

        _migrationService.Verify(x => x.ApplyMigrationsAsync(), Times.Exactly(2));
    }
}
