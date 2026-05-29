using System.Net;

namespace CodingCell.YARPad;

/// <summary>
/// Configuration options for LAN-only access restriction.
/// </summary>
public class LanAccessOptions
{
    /// <summary>
    /// List of trusted proxy IP addresses. When behind a reverse proxy/load balancer,
    /// X-Forwarded-For headers will only be trusted from these IPs.
    /// Example: ["10.0.0.1", "192.168.1.1"]
    /// </summary>
    public List<string> TrustedProxies { get; set; } = [];

    /// <summary>
    /// List of trusted proxy networks in CIDR notation.
    /// Example: ["10.0.0.0/8", "172.16.0.0/12"]
    /// </summary>
    public List<string> TrustedNetworks { get; set; } = [];

    /// <summary>
    /// Additional allowed CIDR ranges beyond the default private ranges.
    /// Use this to allow global unicast IPv6 prefixes used on your LAN.
    /// Example: ["2a02:1234::/48", "203.0.113.0/24"]
    /// </summary>
    public List<string> AdditionalAllowedRanges { get; set; } = [];

    /// <summary>
    /// When true, includes the default private IPv4 ranges (10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16)
    /// and IPv6 ULA (fc00::/7) and link-local (fe80::/10) ranges.
    /// Default is true.
    /// </summary>
    public bool IncludeDefaultPrivateRanges { get; set; } = true;

    /// <summary>
    /// When true, allows loopback addresses (127.0.0.1, ::1).
    /// Default is true.
    /// </summary>
    public bool AllowLoopback { get; set; } = true;

    /// <summary>
    /// Maximum number of proxies to process from X-Forwarded-For header.
    /// Helps prevent header spoofing attacks.
    /// Default is 1.
    /// </summary>
    public int? ForwardLimit { get; set; }

    internal IReadOnlyList<IPNetwork> GetParsedTrustedNetworks()
    {
        var networks = new List<IPNetwork>();

        foreach (var cidr in TrustedNetworks)
        {
            if (IPNetwork.TryParse(cidr, out var network))
                networks.Add(network);
        }

        return networks;
    }

    internal IReadOnlyList<IPAddress> GetParsedTrustedProxies()
    {
        var proxies = new List<IPAddress>();

        foreach (var ip in TrustedProxies)
        {
            if (IPAddress.TryParse(ip, out var address))
                proxies.Add(address);
        }

        return proxies;
    }

    internal IReadOnlyList<IPNetwork> GetAllAllowedNetworks()
    {
        var networks = new List<IPNetwork>();

        if (IncludeDefaultPrivateRanges)
        {
            // IPv4 private ranges
            networks.Add(IPNetwork.Parse("10.0.0.0/8"));       // Class A private
            networks.Add(IPNetwork.Parse("172.16.0.0/12"));    // Class B private
            networks.Add(IPNetwork.Parse("192.168.0.0/16"));   // Class C private

            // IPv6 private/local ranges
            networks.Add(IPNetwork.Parse("fc00::/7"));         // Unique Local Addresses (ULA)
            networks.Add(IPNetwork.Parse("fe80::/10"));        // Link-local IPv6
        }

        foreach (var cidr in AdditionalAllowedRanges)
        {
            if (IPNetwork.TryParse(cidr, out var network))
                networks.Add(network);
        }

        return networks;
    }
}
