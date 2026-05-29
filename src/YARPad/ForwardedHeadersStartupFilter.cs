using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

internal sealed class ForwardedHeadersStartupFilter : IStartupFilter
{
    private readonly YARPadOptions _options;

    public ForwardedHeadersStartupFilter(IOptions<YARPadOptions> options)
    {
        _options = options.Value;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            var lanOptions = _options.LanAccess;
            var trustedProxies = lanOptions.GetParsedTrustedProxies();
            var trustedNetworks = lanOptions.GetParsedTrustedNetworks();

            if (trustedProxies.Count > 0 || trustedNetworks.Count > 0)
            {
                var forwardedHeadersOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | 
                        ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix,
                    ForwardLimit = lanOptions.ForwardLimit
                };

                forwardedHeadersOptions.KnownProxies.Clear();
                forwardedHeadersOptions.KnownIPNetworks.Clear();

                foreach (var proxy in trustedProxies)
                    forwardedHeadersOptions.KnownProxies.Add(proxy);

                foreach (var network in trustedNetworks)
                    forwardedHeadersOptions.KnownIPNetworks.Add(network);

                app.UseForwardedHeaders(forwardedHeadersOptions);
            }

            next(app);
        };
    }
}
