using System.Net;

namespace CodingCell.YARPad;

/// <summary>
/// Service for validating LAN access based on IP addresses.
/// </summary>
internal interface ILanAccessValidator
{
    /// <summary>
    /// Checks if the specified IP address is allowed based on LAN access configuration.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <returns>True if the IP address is allowed; otherwise, false.</returns>
    bool IsAllowedAddress(IPAddress ipAddress);
}
