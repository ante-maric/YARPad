using System.Security.Cryptography.X509Certificates;

namespace CodingCell.YARPad.Hosting;

/// <summary>
/// In-memory store for TLS certificates used by Kestrel during SNI selection.
/// </summary>
internal interface ICertificateStore
{
    /// <summary>Adds or replaces the certificate for the given domain name (supports wildcards).</summary>
    Task<X509Certificate2> AddCertAsync(string domainName, X509Certificate2 certificate);

    /// <summary>Returns the best-matching certificate for the given domain name, or null if none is found.</summary>
    Task<X509Certificate2?> GetCertAsync(string domainName);

    /// <summary>Removes the certificate registered for the given domain name.</summary>
    Task<bool> RemoveCertAsync(string domainName);

    /// <summary>Returns true when a certificate covering the given domain name exists in the store.</summary>
    Task<bool> ContainsCertAsync(string domainName);

    /// <summary>Returns all domain names for which certificates are currently stored.</summary>
    Task<IEnumerable<string>> GetAllDomainsAsync();
}
