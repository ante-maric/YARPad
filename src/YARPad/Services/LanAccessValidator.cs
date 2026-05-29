using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

/// <summary>
/// Service for validating LAN access based on IP addresses and configured networks.
/// </summary>
internal class LanAccessValidator : ILanAccessValidator
{
    private readonly IReadOnlyList<IPNetwork> _allowedNetworks;
    private readonly bool _allowLoopback;
    private readonly ILogger<LanAccessValidator> _logger;

    public LanAccessValidator(IOptions<YARPadOptions> options, ILogger<LanAccessValidator> logger)
    {
        var lanOptions = options.Value.LanAccess;
        _allowedNetworks = lanOptions.GetAllAllowedNetworks();
        _allowLoopback = lanOptions.AllowLoopback;
        _logger = logger;

        logger.LogInformation("LAN access validator initialized with {NetworkCount} allowed networks (AllowLoopback: {AllowLoopback})", 
            _allowedNetworks.Count, _allowLoopback);
    }

    /// <inheritdoc/>
    public bool IsAllowedAddress(IPAddress ipAddress)
    {
        if (_allowLoopback && IPAddress.IsLoopback(ipAddress))
            return true;

        var addressToCheck = ipAddress.IsIPv4MappedToIPv6
            ? ipAddress.MapToIPv4()
            : ipAddress;

        foreach (var network in _allowedNetworks)
        {
            if (network.Contains(addressToCheck))
                return true;
        }

        _logger.LogWarning("Denying IP address {IPAddress} - not in allowed networks", ipAddress);
        return false;
    }
}
