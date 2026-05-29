using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CodingCell.YARPad;

/// <summary>
/// Middleware that restricts access to requests originating from LAN addresses only.
/// Supports reverse proxy scenarios when configured with trusted proxies/networks.
/// </summary>
internal class LanOnlyAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILanAccessValidator _lanAccessValidator;
    private readonly ILogger<LanOnlyAccessMiddleware> _logger;

    public LanOnlyAccessMiddleware(
        RequestDelegate next,
        ILanAccessValidator lanAccessValidator,
        ILogger<LanOnlyAccessMiddleware> logger)
    {
        _next = next;
        _lanAccessValidator = lanAccessValidator;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        if (remoteIpAddress is null || !_lanAccessValidator.IsAllowedAddress(remoteIpAddress))
        {
            _logger.LogWarning("Public access denied - RemoteIP: {RemoteIP}", remoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Access denied.");
            return;
        }

        await _next(context);
    }
}
