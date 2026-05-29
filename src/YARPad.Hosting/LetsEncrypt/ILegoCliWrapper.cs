namespace CodingCell.YARPad.Hosting.LetsEncrypt;

internal interface ILegoCliWrapper
{
    Task<bool> RenewAsync(CancellationToken cancellationToken = default);
    Task<bool> RenewAsync(IEnumerable<string> certificateNames, CancellationToken cancellationToken = default);
    Task<bool> RunAsync(CancellationToken cancellationToken = default);
    Task<bool> RunAsync(Dictionary<string, CertificateOptions> certificates, CancellationToken cancellationToken = default);
}