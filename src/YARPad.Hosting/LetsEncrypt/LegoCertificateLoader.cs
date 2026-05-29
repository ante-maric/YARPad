using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad.Hosting.LetsEncrypt;

internal sealed class LegoCertificateLoader(
    IOptions<YARPadProxyOptions> options,
    IWebHostEnvironment webHostEnvironment,
    ICertificateStore certStore,
    ILogger<LegoCertificateLoader> logger) : ICertificateLoader
{
    private readonly LetsEncryptOptions _letsEncryptOptions = options.Value.LetsEncrypt;
    private readonly string _dataPath = Path.Combine(webHostEnvironment.GetDataRootFullPath(options.Value), options.Value.LetsEncrypt.DataPath);

    public async Task LoadAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, cert) in _letsEncryptOptions.Certificates)
            await LoadAsync(name, cert, cancellationToken);
    }

    public async Task<bool> LoadAsync(string certName, CertificateOptions cert, CancellationToken cancellationToken = default)
    {
        var certFile = FindCertFile(certName);
        if (certFile is null)
        {
            logger.LogDebug("No certificate file found for '{Name}', skipping load.", certName);
            return false;
        }

        var keyFile = Path.ChangeExtension(certFile, ".key");
        if (!File.Exists(keyFile))
        {
            logger.LogWarning("Certificate file found for '{Name}' but key file '{KeyFile}' is missing.", certName, keyFile);
            return false;
        }

        try
        {
            // CreateFromPemFile with private key - export to PFX bytes and reload to ensure
            // the key is fully usable with SslStream across all platforms.
            using var temp = X509Certificate2.CreateFromPemFile(certFile, keyFile);
            var pfxBytes = temp.Export(X509ContentType.Pfx);
            var x509 = X509CertificateLoader.LoadPkcs12(pfxBytes, null);

            logger.LogInformation(
                "Loaded certificate for '{Name}' (expires {Expiry:yyyy-MM-dd}, domains: {Domains}).",
                certName, x509.NotAfter, string.Join(", ", cert.Domains));

            foreach (var domain in cert.Domains)
                await certStore.AddCertAsync(domain, x509);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load certificate for '{Name}' from '{CertFile}'.", certName, certFile);
            return false;
        }
    }

    public bool HasCertificate(string certName) => FindCertFile(certName) is not null;

    private string? FindCertFile(string certName)
    {
        var certDir = Path.Combine(_dataPath, certName, "certificates");
        if (!Directory.Exists(certDir))
            return null;

        return Directory.EnumerateFiles(certDir, "*.crt")
            .FirstOrDefault(f => !f.EndsWith(".issuer.crt", StringComparison.OrdinalIgnoreCase));
    }
}
