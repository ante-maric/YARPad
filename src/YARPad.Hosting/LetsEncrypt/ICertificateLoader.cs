namespace CodingCell.YARPad.Hosting.LetsEncrypt;

internal interface ICertificateLoader
{
    bool HasCertificate(string certName);
    Task LoadAllAsync(CancellationToken cancellationToken = default);
    Task<bool> LoadAsync(string certName, CertificateOptions cert, CancellationToken cancellationToken = default);
}