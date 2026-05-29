#if DEBUG
using CodingCell.YARPad.Hosting.TestExtensions;
#endif
using CodingCell.YARPad.Hosting;

namespace CodingCell.YARPad.Proxy;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureYARPadProxy();

        builder.Services.AddYARPad(builder.Configuration, version: System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
        var proxyBuilder = builder.Services.AddReverseProxy();

#if DEBUG
        proxyBuilder.AddTestTransforms();
        builder.Services.AddTestPolicies();
#endif

        var app = builder.Build();

        app.MapAcmeChallenge();
        app.MapYARPad();
        app.MapReverseProxy();

        app.Run();
    }
}
