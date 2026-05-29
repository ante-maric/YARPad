using CodingCell.YARPad.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps YARPad middleware and endpoints.
    /// </summary>
    public static void MapYARPad(this WebApplication app)
    {
        var coordinator = app.Services.GetRequiredService<YarpConfigurationCoordinator>();
        coordinator.Initialize();

        var options = app.Services.GetRequiredService<IOptions<YARPadOptions>>().Value;
        var pathPrefix = options.GetNormalizedPathPrefix();

        app.MapWhen(ctx =>
        {
            if (!ctx.Request.Path.StartsWithSegments(pathPrefix))
                return false;

            if (options.Hosts.Count == 0)
                return true;

            return ctx.Request.Host.HasValue && options.Hosts.Contains(ctx.Request.Host.Host);
        }, yarpad =>
        {
            // Undo any endpoint pre-selection (e.g., YARP catch-all) so branch routing can run
            yarpad.Use((ctx, next) =>
            {
                ctx.SetEndpoint(null);
                ctx.Request.RouteValues.Clear();
                return next(ctx);
            });

            yarpad.UsePathBase(pathPrefix);

            // Only restrict non-LAN access at middleware level when explicitly enabled
            if (!options.IsLanOnlyAccessDisabled)
                yarpad.UseMiddleware<LanOnlyAccessMiddleware>();

            yarpad.UseStaticFiles();

            yarpad.UseRouting();
            yarpad.UseAntiforgery();
            yarpad.UseAuthentication();
            yarpad.UseAuthorization();
            
            yarpad.UseEndpoints(endpoints =>
            {
                var registry = app.Services.GetService<RazorAssemblyRegistry>();

                var razorComponents = endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();

                if (registry?.Assemblies.Count > 0)
                    razorComponents.AddAdditionalAssemblies([.. registry.Assemblies]);

                razorComponents.WithOrder(int.MinValue);

                endpoints.MapAdditionalIdentityEndpoints()
                    .WithOrder(int.MinValue);

                endpoints.MapFallback(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Not found in YARPad.");
                });
            });
        });
    }
}
