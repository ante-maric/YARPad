using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

internal class YarpConfig : IProxyConfig, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public IReadOnlyList<RouteConfig> Routes { get; set; } = [];

    public IReadOnlyList<ClusterConfig> Clusters { get; set; } = [];

    public IChangeToken ChangeToken { get; set; }

    public YarpConfig()
    {
        ChangeToken = new CancellationChangeToken(_cts.Token);
    }

    public void SignalChange()
    {
        _cts.Cancel();
    }

    public void Dispose()
    {
        _cts.Dispose();
    }
}