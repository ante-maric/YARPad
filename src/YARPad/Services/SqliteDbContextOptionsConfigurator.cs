using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

internal sealed class SqliteDbContextOptionsConfigurator : IDbContextOptionsConfigurator
{
    private readonly IOptions<YARPadOptions> _options;

    public SqliteDbContextOptionsConfigurator(IOptions<YARPadOptions> options)
    {
        _options = options;
    }

    public void Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
        => options.UseSqlite(_options.Value.ConnectionString);
}
