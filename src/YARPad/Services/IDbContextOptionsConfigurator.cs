using Microsoft.EntityFrameworkCore;

namespace CodingCell.YARPad;

/// <summary>
/// Configures <see cref="DbContextOptionsBuilder"/> for the application's database context.
/// Replace the default registration to switch the database provider.
/// </summary>
public interface IDbContextOptionsConfigurator
{
    void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder options);
}
