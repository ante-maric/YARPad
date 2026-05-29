using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace CodingCell.YARPad.Hosting;

public sealed class WildcardRuntimeCertificateStore : ICertificateStore
{
    private readonly ConcurrentDictionary<string, X509Certificate2> _exactCerts =
        new(StringComparer.OrdinalIgnoreCase);

    // Stores "*.some.eu" as "some.eu"
    private readonly ConcurrentDictionary<string, X509Certificate2> _wildcardCerts =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<X509Certificate2> AddCertAsync(string domainName, X509Certificate2 certificate)
    {
        AddToStore(_exactCerts, _wildcardCerts, domainName, certificate);
        return Task.FromResult(certificate);
    }

    public Task<X509Certificate2?> GetCertAsync(string domainName)
    {
        return Task.FromResult(FindBestMatch(_exactCerts, _wildcardCerts, domainName));
    }

    public Task<bool> RemoveCertAsync(string domainName)
    {
        return Task.FromResult(RemoveFromStore(_exactCerts, _wildcardCerts, domainName));
    }

    public Task<bool> ContainsCertAsync(string domainName)
    {
        return Task.FromResult(FindBestMatch(_exactCerts, _wildcardCerts, domainName) is not null);
    }

    public Task<IEnumerable<string>> GetAllDomainsAsync()
    {
        var all = new List<string>(_exactCerts.Count + _wildcardCerts.Count);

        foreach (var key in _exactCerts.Keys)
            all.Add(key);

        foreach (var key in _wildcardCerts.Keys)
            all.Add("*." + key);

        return Task.FromResult<IEnumerable<string>>(all);
    }

    private static void AddToStore(
        ConcurrentDictionary<string, X509Certificate2> exactStore,
        ConcurrentDictionary<string, X509Certificate2> wildcardStore,
        string domainName,
        X509Certificate2 certificate)
    {
        if (string.IsNullOrEmpty(domainName))
            return;

        var normalized = Normalize(domainName);

        if (IsWildcard(normalized))
        {
            wildcardStore.AddOrUpdate(
                normalized[2..],
                certificate,
                (_, current) => SelectPreferred(current, certificate));
        }
        else
        {
            exactStore.AddOrUpdate(
                normalized,
                certificate,
                (_, current) => SelectPreferred(current, certificate));
        }
    }

    private static bool RemoveFromStore(
        ConcurrentDictionary<string, X509Certificate2> exactStore,
        ConcurrentDictionary<string, X509Certificate2> wildcardStore,
        string domainName)
    {
        if (string.IsNullOrEmpty(domainName))
            return false;

        var normalized = Normalize(domainName);

        if (IsWildcard(normalized))
        {
            var removed = wildcardStore.TryRemove(normalized[2..], out var wCert);
            if (removed) wCert!.Dispose();
            return removed;
        }

        var result = exactStore.TryRemove(normalized, out var eCert);
        if (result) eCert!.Dispose();
        return result;
    }

    private static X509Certificate2? FindBestMatch(
        ConcurrentDictionary<string, X509Certificate2> exactStore,
        ConcurrentDictionary<string, X509Certificate2> wildcardStore,
        string domainName)
    {
        if (string.IsNullOrEmpty(domainName))
            return null;

        var host = Normalize(domainName);

        if (exactStore.TryGetValue(host, out var exact))
            return exact;

        var firstDot = host.IndexOf('.');
        if (firstDot <= 0 || firstDot == host.Length - 1)
            return null;

        return wildcardStore.TryGetValue(host[(firstDot + 1)..], out var wildcard) ? wildcard : null;
    }

    private static X509Certificate2 SelectPreferred(X509Certificate2 current, X509Certificate2 incoming) =>
        incoming.NotAfter >= current.NotAfter ? incoming : current;

    private static bool IsWildcard(string host) =>
        host.Length > 2 && host[0] == '*' && host[1] == '.';

    private static string Normalize(string host) =>
        host is [.., '.'] ? host[..^1] : host;
}
