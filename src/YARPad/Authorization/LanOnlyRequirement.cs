using Microsoft.AspNetCore.Authorization;

namespace CodingCell.YARPad;

/// <summary>
/// Authorization requirement that restricts access to requests originating from LAN addresses only.
/// </summary>
internal class LanOnlyRequirement : IAuthorizationRequirement
{
}
