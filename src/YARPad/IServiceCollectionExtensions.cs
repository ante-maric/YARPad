using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Yarp.ReverseProxy.Configuration;
using CodingCell.YARPad.Components.Account;
using CodingCell.YARPad.Components.Layout;
using CodingCell.YARPad.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CodingCell.YARPad;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddYARPad(this IServiceCollection services, IConfiguration configuration, Action<YARPadOptions>? configure = null, Version? version = null)
        => AddYARPad<YARPadOptions>(services, configuration, configure, version);

    public static IServiceCollection AddYARPad<TOptions>(this IServiceCollection services, IConfiguration configuration, Action<TOptions>? configure = null, Version? version = null)
        where TOptions : YARPadOptions
    {
        services.Configure<YARPadOptions>(configuration.GetSection(YARPadOptions.SECTION_NAME));

        if (configure != null)
            services.Configure(configure);

        services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();
        services.AddHttpContextAccessor();

        services
            .AddScoped<IClusterEditorService, ClusterEditorService>()
            .AddScoped<IRouteEditorService, RouteEditorService>()
            .AddSingleton<IYARPadConfigurationProvider, YARPadConfigurationProvider>()
            .AddSingleton<IYarpConfigProvider, YarpConfigProvider>()
            .AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<IYarpConfigProvider>())
            .AddSingleton<YarpConfigurationCoordinator>()
            .AddSingleton<IYarpOutputCachePolicyProvider, YarpOutputCachePolicyProvider>()
            .AddSingleton<IYarpRateLimiterPolicyProvider, YarpRateLimiterPolicyProvider>()
            .AddTransient<IPolicyValidatorFactory, PolicyValidatorFactory>()
            .AddKeyedScoped<IPolicyProvider, AuthorizationPolicyProvider>(PolicyType.Authorization)
            .AddKeyedScoped<IPolicyProvider, TimeoutPolicyProvider>(PolicyType.Timeout)
            .AddKeyedScoped<IPolicyProvider, RateLimitingPolicyProvider>(PolicyType.RateLimiter)
            .AddKeyedScoped<IPolicyProvider, CorsPolicyProvider>(PolicyType.Cors)
            .AddKeyedScoped<IPolicyProvider, OutputCachePolicyProvider>(PolicyType.OutputCache)
            .AddKeyedScoped<IPolicyProvider, LoadBalancingPolicyProvider>(PolicyType.LoadBalancing)
            .AddKeyedScoped<IPolicyProvider, SessionAffinityPolicyProvider>(PolicyType.SessionAffinity)
            .AddKeyedScoped<IPolicyProvider, SessionAffinityFailurePolicyProvider>(PolicyType.SessionAffinityFailure)
            .AddKeyedScoped<IPolicyProvider, ActiveHealthCheckPolicyProvider>(PolicyType.ActiveHealthCheck)
            .AddKeyedScoped<IPolicyProvider, PassiveHealthCheckPolicyProvider>(PolicyType.PassiveHealthCheck)
            .AddKeyedScoped<IPolicyProvider, AvailableDestinationsPolicyProvider>(PolicyType.AvailableDestination)
            .AddScoped<IUnifiedPolicyProvider, UnifiedPolicyProvider>()
            .AddSingleton<ILanAccessValidator, LanAccessValidator>()
            .AddSingleton<IAuthorizationHandler, LanOnlyAuthorizationHandler>();

        services.TryAddSingleton<IConfigChangeNotifier, NoOpConfigChangeNotifier>();

        services.AddAutoMapper((x, y) => { }, typeof(AutoMapperProfile).Assembly);

        services.AddValidatorsFromAssemblyContaining<YARPadConfiguration>();

        services
            .AddStateStore<ConfigurationProfileState, StateStore<ConfigurationProfileState>>(new ConfigurationProfileState([], null))
            .AddStateStore<YarpConfigStatusState, StateStore<YarpConfigStatusState>>(new YarpConfigStatusState(Guid.Empty, YARPadConfigurationStatus.Loading, [], DateTime.UtcNow))
            .AddStateStore<ThemeState, CookieStateStore<ThemeState>>(new(true), ServiceLifetime.Scoped)
            .AddStateStore<CurrentConfigurationProfileState, StateStore<CurrentConfigurationProfileState>>(new(null), ServiceLifetime.Scoped)
            .AddStateStore<AppInfoState, StateStore<AppInfoState>>(new(version ?? System.Reflection.Assembly.GetExecutingAssembly().GetName().Version), ServiceLifetime.Scoped)
            .AddStateStore<MainState, MainStateStore>(ServiceLifetime.Scoped);

        services
            .AddCascadingAuthenticationState()
            .AddScoped<IdentityRedirectManager>()
            .AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationConstants.LanOnly,
                policy => policy.AddRequirements(new LanOnlyRequirement()));
        });

        services.TryAddSingleton<IDbContextOptionsConfigurator, SqliteDbContextOptionsConfigurator>();

        services
            .AddDbContext<ApplicationDbContext>((sp, dbOptions) =>
                sp.GetRequiredService<IDbContextOptionsConfigurator>().Configure(sp, dbOptions))
            .AddDatabaseDeveloperPageExceptionFilter();

        services
            .Configure<IdentityOptions>(configuration.GetSection("Identity"))
            .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.TryAddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        services.TryAddScoped<IDatabaseMigrationService, BuiltInDatabaseMigrationService>();
        services.AddHostedService<MigrationHostedService>();
        services.AddHostedService<ConfigChangeNotifierHostedService>();

        // Register custom authorization middleware result handler to prevent
        // authentication challenges for LanOnly failures (avoids redirect loops)
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, LanOnlyAuthorizationMiddlewareResultHandler>();

        // Register IStartupFilter to ensure UseForwardedHeaders runs first in the pipeline
        // This guarantees correct IP resolution for LanOnly authorization, regardless of user's middleware ordering
        services.AddSingleton<IStartupFilter, ForwardedHeadersStartupFilter>();

        services.AddSingleton<INavMenuContributor, ConfigurationNavMenuContributor>();
        services.TryAddSingleton(new RazorAssemblyRegistry());
        services.TryAddScoped<IYARPadExtraFeatures, YARPadNoExtraFeatures>();

        return services;
    }

    public static IServiceCollection AddStateStore<TState, TStore>(
        this IServiceCollection services,
        TState initialState,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TState : class
        where TStore : class, IStoreReader<TState>, IStoreWriter<TState>, IStateStore<TState>
    {
        services.Add(new ServiceDescriptor(typeof(TState), sp => initialState, lifetime));
        services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreReader<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreWriter<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStateStore<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));

        return services;
    }

    public static IServiceCollection AddStateStore<TState, TStore>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TState : class
        where TStore : class, IStoreReader<TState>, IStoreWriter<TState>, IStateStore<TState>
    {
        services.Add(new ServiceDescriptor(typeof(TState), typeof(TState), lifetime));
        services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreReader<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreWriter<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStateStore<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));

        return services;
    }
}
