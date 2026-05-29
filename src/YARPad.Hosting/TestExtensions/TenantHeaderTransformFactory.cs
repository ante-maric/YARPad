#if DEBUG
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class TenantHeaderTransformFactory : ITransformFactory
{
    private const string TransformName = "TenantHeader";     // key in config
    private const string RouteParamKey = "TenantRouteParam"; // key in config
    private const string HeaderName = "X-Tenant";            // header we will set

    public bool Validate(
        TransformRouteValidationContext context,
        IReadOnlyDictionary<string, string> transformValues)
    {
        // Check if this transform applies
        if (!transformValues.TryGetValue(TransformName, out var enabledValue))
        {
            return false; // not ours
        }
        
        // Basic value check
        if (!string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Errors.Add(new ArgumentException(
                $"{TransformName} must be 'true' when specified."));
        }

        // Require TenantRouteParam
        if (!transformValues.TryGetValue(RouteParamKey, out var routeParamName) ||
            string.IsNullOrWhiteSpace(routeParamName))
        {
            context.Errors.Add(new ArgumentException(
                $"{RouteParamKey} is required and must be non-empty for {TransformName}."));
        }

        return true; // we matched this transform dictionary
    }

    public bool Build(
        TransformBuilderContext context,
        IReadOnlyDictionary<string, string> transformValues)
    {
        // Same matching logic as Validate
        if (!transformValues.TryGetValue(TransformName, out var enabledValue) ||
            !string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!transformValues.TryGetValue(RouteParamKey, out var routeParamName) ||
            string.IsNullOrWhiteSpace(routeParamName))
        {
            throw new ArgumentException(
                $"{RouteParamKey} is required and must be non-empty for {TransformName}.");
        }

        // Add the actual request transform
        context.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;

            if (httpContext.Request.RouteValues.TryGetValue(routeParamName, out var valueObj) &&
                valueObj is not null)
            {
                var tenantValue = Convert.ToString(valueObj);
                if (!string.IsNullOrEmpty(tenantValue))
                {
                    // Ensure we don't accumulate multiple headers
                    transformContext.ProxyRequest.Headers.Remove(HeaderName);
                    transformContext.ProxyRequest.Headers.Add(HeaderName, tenantValue);
                }
            }

            return default; // ValueTask
        });

        return true;
    }
}
#endif
