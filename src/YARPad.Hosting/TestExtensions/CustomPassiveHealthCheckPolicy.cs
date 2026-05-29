#if DEBUG
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomPassiveHealthCheckPolicy : IPassiveHealthCheckPolicy
{
    public string Name => "CustomPHCP";

    public void RequestProxied(HttpContext context, ClusterState cluster, DestinationState destination)
    {
        throw new NotImplementedException();
    }
}
#endif
