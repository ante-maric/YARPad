using System.Security.Authentication;

namespace CodingCell.YARPad;

public sealed record HttpClientModel
{
    public IReadOnlyCollection<SslProtocols> SslProtocols { get; set; } = new HashSet<SslProtocols>();

    public bool? DangerousAcceptAnyServerCertificate { get; set; }

    public int? MaxConnectionsPerServer { get; set; }

    public WebProxyModel WebProxy { get; set; } = new();

    public bool? EnableMultipleHttp2Connections { get; set; }

    public string? RequestHeaderEncoding { get; set; }

    public string? ResponseHeaderEncoding { get; set; }
}
