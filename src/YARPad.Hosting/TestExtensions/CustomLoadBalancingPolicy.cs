#if DEBUG
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomLoadBalancingPolicy : ILoadBalancingPolicy
{
    public string Name => "CustomLB";

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        throw new NotImplementedException();
    }
}
#endif
