using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad.Hosting.LetsEncrypt;

internal class LegoCliWrapper(IWebHostEnvironment webHostEnvironment, IOptions<YARPadProxyOptions> options, ILogger<LegoCliWrapper> logger) : ILegoCliWrapper
{
    private const string LEGO_CLI_PATH = "tools/lego";

    private readonly YARPadProxyOptions _options = options.Value;
    private readonly string _dataPath = Path.Combine(webHostEnvironment.GetDataRootFullPath(options.Value), options.Value.LetsEncrypt.DataPath);

    public Task<bool> RunAsync(CancellationToken cancellationToken = default) =>
        RunAsync(_options.LetsEncrypt.Certificates, cancellationToken);

    public async Task<bool> RunAsync(Dictionary<string, CertificateOptions> certificates, CancellationToken cancellationToken = default)
    {
        var success = true;
        foreach (var cert in certificates)
        {
            var args = BuildRunArguments(cert.Key, cert.Value);
            if (!await ExecuteAsync(args, cert.Value.EnvironmentVariables, cancellationToken))
                success = false;
        }

        return success;
    }

    public Task<bool> RenewAsync(CancellationToken cancellationToken = default) =>
        RenewAsync(_options.LetsEncrypt.Certificates.Keys, cancellationToken);

    public async Task<bool> RenewAsync(IEnumerable<string> certificateNames, CancellationToken cancellationToken = default)
    {
        var success = true;
        foreach (var name in certificateNames)
        {
            var args = BuildRenewArguments(_dataPath, name, _options.LetsEncrypt.Certificates[name]);
            if (!await ExecuteAsync(args, _options.LetsEncrypt.Certificates[name].EnvironmentVariables, cancellationToken))
                success = false;
        }

        return success;
    }

    private string BuildGlobalArguments(string rootPath, string certificateName, CertificateOptions cert)
    {
        var sb = new StringBuilder();

        var path = Path.Combine(rootPath, certificateName);
        sb.Append($"--path \"{path}\"");

        if (_options.LetsEncrypt.UseStagingServer)
            sb.Append($" --server {_options.LetsEncrypt.GetStagingServerUrl()}");

        sb.Append($" --email {cert.Email}");
        if (cert.AcmeChallengeType == AcmeChallengeType.Dns01)
            sb.Append($" --dns {cert.DnsProvider}");
        else
            sb.Append($" --http --http.webroot=\"{webHostEnvironment.GetAcmeChallengeRootPath(_options)}\"");
        sb.Append(" --accept-tos");

        foreach (var domain in cert.Domains)
            sb.Append($" -d \"{domain}\"");

        return sb.ToString();
    }

    private string BuildRunArguments(string certificateName, CertificateOptions cert)
    {
        var global = BuildGlobalArguments(_dataPath, certificateName, cert);
        return $"{global} run";
    }

    private string BuildRenewArguments(string rootPath, string certificateName, CertificateOptions cert)
    {
        var global = BuildGlobalArguments(_dataPath, certificateName, cert);
        return $"{global} renew";
    }

    private async Task<bool> ExecuteAsync(string arguments, Dictionary<string, string> environmentVariables, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing: {CliPath} {Arguments}", LEGO_CLI_PATH, arguments);

        var psi = new ProcessStartInfo
        {
            FileName = LEGO_CLI_PATH,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var environmentVariable in environmentVariables)
            psi.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);

        using var process = Process.Start(psi);
        if (process is null)
        {
            logger.LogError("Failed to start lego process at {CliPath}", LEGO_CLI_PATH);
            return false;
        }

        process.OutputDataReceived += OnProcessOutputReceived;
        process.ErrorDataReceived += OnProcessOutputReceived;

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        process.OutputDataReceived -= OnProcessOutputReceived;
        process.ErrorDataReceived -= OnProcessOutputReceived;

        if (process.ExitCode != 0)
        {
            logger.LogError("lego exited with code {ExitCode}", process.ExitCode);
            return false;
        }

        return true;
    }

    private void OnProcessOutputReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
            logger.LogInformation(e.Data);
    }
}
