#if DEBUG
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomSessionAffinityPolicy : ISessionAffinityPolicy
{
    public string Name => "CustomSA";

    public void AffinitizeResponse(HttpContext context, ClusterState cluster, SessionAffinityConfig config, DestinationState destination)
    {
        throw new NotImplementedException();
    }

    public AffinityResult FindAffinitizedDestinations(HttpContext context, ClusterState cluster, SessionAffinityConfig config, IReadOnlyList<DestinationState> destinations)
    {
        throw new NotImplementedException();
    }
}
#endif
