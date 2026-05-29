using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CodingCell.YARPad;

/// <summary>
/// Authorization handler that validates LAN-only access based on the remote IP address.
/// Supports reverse proxy scenarios when configured with trusted proxies/networks.
/// </summary>
internal class LanOnlyAuthorizationHandler : AuthorizationHandler<LanOnlyRequirement>
{
    private readonly ILanAccessValidator _lanAccessValidator;
    private readonly ILogger<LanOnlyAuthorizationHandler> _logger;

    public LanOnlyAuthorizationHandler(
        ILanAccessValidator lanAccessValidator,
        ILogger<LanOnlyAuthorizationHandler> logger)
    {
        _lanAccessValidator = lanAccessValidator;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LanOnlyRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        if (httpContext is null)
        {
            _logger.LogWarning("LAN-only authorization failed: HttpContext is null");
            context.Fail();

            return Task.CompletedTask;
        }

        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
        {
            _logger.LogWarning("LAN-only authorization failed: RemoteIpAddress is null");
            context.Fail();

            return Task.CompletedTask;
        }

        if (_lanAccessValidator.IsAllowedAddress(remoteIpAddress))
            context.Succeed(requirement);
        else
        {
            _logger.LogWarning("LAN-only authorization failed: IP address {RemoteIpAddress} is not allowed", remoteIpAddress);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
