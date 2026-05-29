#if DEBUG
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomSessionAffinityFailurePolicy : IAffinityFailurePolicy
{
    public string Name => "CustomSAF1";

    public Task<bool> Handle(HttpContext context, ClusterState cluster, AffinityStatus affinityStatus)
    {
        throw new NotImplementedException();
    }
}
#endif
