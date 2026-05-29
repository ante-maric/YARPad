namespace CodingCell.YARPad;

public interface IDatabaseMigrationService
{
    Task ApplyMigrationsAsync();
}
