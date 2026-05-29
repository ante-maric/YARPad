#if DEBUG
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public class CustomActiveHealthCheckPolicy : IActiveHealthCheckPolicy
{
    public string Name => "CustmAHCP";

    public void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults)
    {
        throw new NotImplementedException();
    }
}
#endif