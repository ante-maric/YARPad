using System.Security.Authentication;

namespace CodingCell.YARPad;

internal static class SslProtocolsExtensions
{
    extension(IEnumerable<SslProtocols> protocols)
    {
        public SslProtocols ToSingleFlag()
        {
            return protocols.Aggregate(SslProtocols.None, (current, protocol) => current | protocol);
        }
    }
}