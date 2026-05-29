#if DEBUG
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class MyTransformProvider : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        if (context.Cluster.Metadata?.TryGetValue("CustomMetadata", out var value) == true)
        {
            if (string.IsNullOrEmpty(value))
            {
                context.Errors.Add(new ArgumentException(
                    "A non-empty CustomMetadata value is required"));
            }
        }
    }

    public void ValidateRoute(TransformRouteValidationContext context)
    {
        if (context.Route.Metadata?.TryGetValue("CustomMetadata", out var value) == true)
        {
            if (string.IsNullOrEmpty(value))
            {
                context.Errors.Add(new ArgumentException(
                    "A non-empty CustomMetadata value is required"));
            }
        }
    }
}
#endif
