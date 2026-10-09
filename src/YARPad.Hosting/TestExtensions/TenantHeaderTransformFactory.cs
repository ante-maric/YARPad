#if DEBUG
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class TenantHeaderTransformFactory : ITransformFactory
{
    private const string TRANSFORM_NAME = "TenantHeader";     // key in config
    private const string ROUTE_PARAM_KEY = "TenantRouteParam"; // key in config
    private const string HEADER_NAME = "X-Tenant";            // header we will set

    public bool Validate(
        TransformRouteValidationContext context,
        IReadOnlyDictionary<string, string> transformValues)
    {
        // Check if this transform applies
        if (!transformValues.TryGetValue(TRANSFORM_NAME, out var enabledValue))
        {
            return false; // not ours
        }
        
        // Basic value check
        if (!string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Errors.Add(new ArgumentException(
                $"{TRANSFORM_NAME} must be 'true' when specified."));
        }

        // Require TenantRouteParam
        if (!transformValues.TryGetValue(ROUTE_PARAM_KEY, out var routeParamName) ||
            string.IsNullOrWhiteSpace(routeParamName))
        {
            context.Errors.Add(new ArgumentException(
                $"{ROUTE_PARAM_KEY} is required and must be non-empty for {TRANSFORM_NAME}."));
        }

        return true; // we matched this transform dictionary
    }

    public bool Build(
        TransformBuilderContext context,
        IReadOnlyDictionary<string, string> transformValues)
    {
        // Same matching logic as Validate
        if (!transformValues.TryGetValue(TRANSFORM_NAME, out var enabledValue) ||
            !string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!transformValues.TryGetValue(ROUTE_PARAM_KEY, out var routeParamName) ||
            string.IsNullOrWhiteSpace(routeParamName))
        {
            throw new ArgumentException(
                $"{ROUTE_PARAM_KEY} is required and must be non-empty for {TRANSFORM_NAME}.");
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
                    transformContext.ProxyRequest.Headers.Remove(HEADER_NAME);
                    transformContext.ProxyRequest.Headers.Add(HEADER_NAME, tenantValue);
                }
            }

            return default; // ValueTask
        });

        return true;
    }
}
#endif
