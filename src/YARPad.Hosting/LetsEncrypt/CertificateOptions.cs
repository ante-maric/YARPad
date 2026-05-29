namespace CodingCell.YARPad.Hosting.LetsEncrypt;

public enum AcmeChallengeType
{
    Http01,
    Dns01
}

public class CertificateOptions
{
    public required string DnsProvider { get; set; }

    public required HashSet<string> Domains { get; set; } = [];

    public required string Email { get; set; }

    public AcmeChallengeType AcmeChallengeType { get; set; } = AcmeChallengeType.Dns01;

    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];
}
