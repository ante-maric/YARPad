using System.Net.Security;
using CodingCell.YARPad.Hosting.LetsEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CodingCell.YARPad.Hosting;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public YARPadProxyOptions ConfigureYARPadProxy() => builder.ConfigureYARPadProxy<YARPadProxyOptions>();

        public TOptions ConfigureYARPadProxy<TOptions>()
            where TOptions : YARPadProxyOptions, new()
        {
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext());

            var proxyOptionSection = builder.Configuration.GetSection(YARPadProxyOptions.SECTION_NAME);
            builder.Services.Configure<TOptions>(proxyOptionSection);
            builder.Services.Configure<YARPadProxyOptions>(proxyOptionSection);

            var proxyOptions = proxyOptionSection.Get<TOptions>() ?? new();

            if (proxyOptions.IsLetsEncryptEnabled)
            {
                builder.Services.AddSingleton<ICertificateStore, WildcardRuntimeCertificateStore>();

                if (proxyOptions.LetsEncrypt.Certificates.Count > 0)
                {
                    builder.Services.AddSingleton<ILegoCliWrapper, LegoCliWrapper>();
                    builder.Services.AddSingleton<ICertificateLoader, LegoCertificateLoader>();
                    builder.Services.AddHostedService<CertificateMaintainerService>();
                }

                builder.WebHost.ConfigureKestrel((context, kestrel) =>
                {
                    var loader = kestrel.Configure(context.Configuration.GetSection("Kestrel"));
                    loader.Endpoint("Https", endpoint =>
                        endpoint.ListenOptions.UseHttps(new TlsHandshakeCallbackOptions
                        {
                            OnConnection = async ctx =>
                            {
                                var store = kestrel.ApplicationServices.GetRequiredService<ICertificateStore>();
                                var cert = await store.GetCertAsync(ctx.ClientHelloInfo.ServerName);
                                return new SslServerAuthenticationOptions { ServerCertificate = cert };
                            }
                        }));
                });
            }

            return proxyOptions;
        }
    }
}
