namespace CodingCell.YARPad.Hosting.LetsEncrypt;

public class LetsEncryptOptions
{
    public const string DefaultStagingServer = "https://acme-staging-v02.api.letsencrypt.org/directory";

    public string DataPath { get; set; } = "LetsEncrypt";

    public bool UseStagingServer { get; set; }

    public string? StagingServerUrl { get; set; }    

    public Dictionary<string, CertificateOptions> Certificates { get; set; } = [];

    internal string GetStagingServerUrl() =>
        string.IsNullOrWhiteSpace(StagingServerUrl) ? DefaultStagingServer : StagingServerUrl;
}
