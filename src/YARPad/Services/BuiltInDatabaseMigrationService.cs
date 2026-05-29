using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CodingCell.YARPad.Data;

namespace CodingCell.YARPad;

public sealed class BuiltInDatabaseMigrationService(ApplicationDbContext db, ILogger<BuiltInDatabaseMigrationService> logger) : IDatabaseMigrationService
{
    public async Task ApplyMigrationsAsync()
    {
        logger.LogInformation("Applying YARPad EF Core migrations for {Context}", nameof(ApplicationDbContext));
        await db.Database.MigrateAsync();
    }
}
