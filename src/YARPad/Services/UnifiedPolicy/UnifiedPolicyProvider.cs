using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad;

internal class UnifiedPolicyProvider(IServiceProvider serviceProvider) : IUnifiedPolicyProvider
{
    public async Task<List<PolicyInfo>> GetPoliciesAsync(Guid configurationProfileID, PolicyType policyType, CancellationToken cancellationToken)
    {
        var policyProvider = serviceProvider.GetRequiredKeyedService<IPolicyProvider>(policyType);
        return await policyProvider.GetPoliciesAsync(configurationProfileID);
    }
}

internal class LoadBalancingPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore) 
    : PolicyProvider(PolicyType.LoadBalancing, configurationStateStore, typeof(LoadBalancingPolicies), x => HelpTexts.GetLoadBalancingPolicyText(x));

internal class SessionAffinityPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.SessionAffinity, configurationStateStore, typeof(SessionAffinityConstants.Policies), x => HelpTexts.GetSessionAffinityPolicyText(x));

internal class SessionAffinityFailurePolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.SessionAffinityFailure, configurationStateStore, typeof(SessionAffinityConstants.FailurePolicies), x => HelpTexts.GetSessionAffinityFailurPolicyText(x));

internal class ActiveHealthCheckPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.ActiveHealthCheck, configurationStateStore, typeof(HealthCheckConstants.ActivePolicy), x => HelpTexts.GetActiveHealthCheckPolicyText(x));

internal class PassiveHealthCheckPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.PassiveHealthCheck, configurationStateStore, typeof(HealthCheckConstants.PassivePolicy), x => HelpTexts.GetPassiveHealthCheckPolicyText(x));

internal class AvailableDestinationsPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.AvailableDestination, configurationStateStore, typeof(HealthCheckConstants.AvailableDestinations), x => HelpTexts.GetAvailableDestinationsPolicyText(x));

internal class AuthorizationPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Authorization, configurationStateStore, typeof(AuthorizationConstants), x => HelpTexts.GetAuthorizationPolicyText(x));

internal class RateLimitingPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.RateLimiter, configurationStateStore, typeof(RateLimitingConstants), x => HelpTexts.GetRateLimitingPolicyText(x));

internal class OutputCachePolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.OutputCache, configurationStateStore, null);

internal class TimeoutPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Timeout, configurationStateStore, typeof(TimeoutPolicyConstants), x => HelpTexts.GetTimeoutPolicyText(x));

internal class CorsPolicyProvider(IStoreReader<ConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Cors, configurationStateStore, typeof(CorsConstants), x => HelpTexts.GetCorsPolicyText(x));
