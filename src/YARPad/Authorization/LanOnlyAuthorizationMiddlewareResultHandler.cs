using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace CodingCell.YARPad;

/// <summary>
/// Custom authorization result handler that prevents authentication challenges
/// for LanOnly authorization failures, returning 403 Forbidden instead.
/// This prevents redirect loops in reverse proxy scenarios.
/// </summary>
internal class LanOnlyAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler _defaultHandler;

    public LanOnlyAuthorizationMiddlewareResultHandler()
    {
        _defaultHandler = new AuthorizationMiddlewareResultHandler();
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded && policy.Requirements.OfType<LanOnlyRequirement>().Any())
        {
            // Return 403 Forbidden without triggering authentication challenge
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        // For all other cases, use default behavior (which may challenge/redirect)
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
