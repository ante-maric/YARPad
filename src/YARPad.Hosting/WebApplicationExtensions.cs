using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad.Hosting;

public static class WebApplicationExtensions
{
    public static void MapAcmeChallenge(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<YARPadProxyOptions>>();
        if (!options.Value.IsLetsEncryptEnabled)
            return;

        var rootPath = app.Environment.GetAcmeChallengePath(options.Value);
        Directory.CreateDirectory(rootPath);

        app.MapGet("/.well-known/acme-challenge/{token}", async (string token, HttpContext ctx, ILogger<WebApplication> logger) =>
        {
            var filePath = Path.Combine(rootPath, token);

            logger.LogInformation("ACME challenge request: {Method} {Path} from {RemoteIp}",
                ctx.Request.Method,
                ctx.Request.Path,
                ctx.Connection.RemoteIpAddress);

            if (!File.Exists(filePath))
            {
                logger.LogWarning("ACME challenge token not found: {Token}", token);
                return Results.NotFound();
            }

            var content = await File.ReadAllTextAsync(filePath);
            return Results.Content(content, "text/plain");
        });
    }
}
