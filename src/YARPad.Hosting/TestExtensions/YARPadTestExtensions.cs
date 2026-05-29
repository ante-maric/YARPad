#if DEBUG
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad.Hosting.TestExtensions;

public static class YARPadTestExtensions
{
    extension(IReverseProxyBuilder builder)
    {
        public IReverseProxyBuilder AddTestTransforms()
        {
            builder
                .AddTransformFactory<TenantHeaderTransformFactory>()
                .AddTransforms<MyTransformProvider>();

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddTestPolicies()
        {
            services.AddSingleton<ILoadBalancingPolicy, CustomLoadBalancingPolicy>();
            services.AddSingleton<ISessionAffinityPolicy, CustomSessionAffinityPolicy>();
            services.AddSingleton<IAffinityFailurePolicy, CustomSessionAffinityFailurePolicy>();
            services.AddSingleton<IActiveHealthCheckPolicy, CustomActiveHealthCheckPolicy>();
            services.AddSingleton<IPassiveHealthCheckPolicy, CustomPassiveHealthCheckPolicy>();
            services.AddSingleton<IAvailableDestinationsPolicy, CustomAvailableDestinationPolicy>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DummyAuthorizationPolicy",
                    policy => policy.RequireAssertion(_ => true));
            });

            services.AddRateLimiter(options =>
            {
                options.AddPolicy("DummyRateLimiterPolicy",
                    _ => RateLimitPartition.GetNoLimiter("DummyRateLimiterPolicy"));
            });

            services.AddOutputCache(options =>
            {
                options.AddPolicy("DummyOutputCachePolicy", policy => policy.NoCache());
            });

            services.AddRequestTimeouts(options =>
            {
                options.AddPolicy("DummyTimeoutPolicy", new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(30)
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("DummyCorsPolicy", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            return services;
        }
    }
}
#endif
