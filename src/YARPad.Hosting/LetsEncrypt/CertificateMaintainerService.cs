using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad.Hosting.LetsEncrypt;

internal sealed class CertificateMaintainerService(
    IOptions<YARPadProxyOptions> options,
    ILegoCliWrapper legoCliWrapper,
    ICertificateLoader certLoader,
    ILogger<CertificateMaintainerService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromDays(1);

    private readonly LetsEncryptOptions _letsEncrypt = options.Value.LetsEncrypt;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_letsEncrypt.Certificates.Count == 0)
        {
            logger.LogInformation("No Let's Encrypt certificates configured, skipping.");
            return;
        }

        await RunCheckAsync(stoppingToken);

        try
        {
            using var timer = new PeriodicTimer(CheckInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunCheckAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is stopping, no action needed.
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running Let's Encrypt certificate check.");

        // Load any already-issued certs first so Kestrel can serve them immediately,
        // even before lego confirms they do not need renewal.
        await certLoader.LoadAllAsync(cancellationToken);

        var needsIssue = new Dictionary<string, CertificateOptions>();
        var needsRenew = new List<string>();

        foreach (var (name, cert) in _letsEncrypt.Certificates)
        {
            if (!certLoader.HasCertificate(name))
            {
                logger.LogInformation("Certificate '{Name}' not found on disk, will issue.", name);
                needsIssue[name] = cert;
            }
            else
            {
                logger.LogInformation("Certificate '{Name}' found, delegating renewal check to lego.", name);
                needsRenew.Add(name);
            }
        }

        if (needsIssue.Count > 0)
        {
            await legoCliWrapper.RunAsync(needsIssue, cancellationToken);
            // Load the newly issued certs.
            await certLoader.LoadAllAsync(cancellationToken);
        }

        if (needsRenew.Count > 0)
        {
            await legoCliWrapper.RenewAsync(needsRenew, cancellationToken);
            // Reload in case lego wrote a renewed cert (it only acts when within 30 days of expiry).
            await certLoader.LoadAllAsync(cancellationToken);
        }
    }
}
