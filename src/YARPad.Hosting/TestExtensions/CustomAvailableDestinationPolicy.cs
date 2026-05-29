#if DEBUG
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomAvailableDestinationPolicy : IAvailableDestinationsPolicy
{
    public string Name => "CustomAD";

    public IReadOnlyList<DestinationState> GetAvailalableDestinations(ClusterConfig config, IReadOnlyList<DestinationState> allDestinations)
    {
        throw new NotImplementedException();
    }
}
#endif