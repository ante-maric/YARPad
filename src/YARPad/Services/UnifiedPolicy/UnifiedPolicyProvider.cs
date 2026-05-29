using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad;

internal class UnifiedPolicyProvider(IServiceProvider serviceProvider) : IUnifiedPolicyProvider
{
    public async Task<List<PolicyInfo>> GetPoliciesAsync(PolicyType policyType, CancellationToken cancellationToken)
    {
        var policyProvider = serviceProvider.GetRequiredKeyedService<IPolicyProvider>(policyType);
        return await policyProvider.GetPoliciesAsync();
    }
}

internal class LoadBalancingPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore) 
    : PolicyProvider(PolicyType.LoadBalancing, configurationStateStore, typeof(LoadBalancingPolicies), x => HelpTexts.GetLoadBalancingPolicyText(x));

internal class SessionAffinityPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.SessionAffinity, configurationStateStore, typeof(SessionAffinityConstants.Policies), x => HelpTexts.GetSessionAffinityPolicyText(x));

internal class SessionAffinityFailurePolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.SessionAffinityFailure, configurationStateStore, typeof(SessionAffinityConstants.FailurePolicies), x => HelpTexts.GetSessionAffinityFailurPolicyText(x));

internal class ActiveHealthCheckPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.ActiveHealthCheck, configurationStateStore, typeof(HealthCheckConstants.ActivePolicy), x => HelpTexts.GetActiveHealthCheckPolicyText(x));

internal class PassiveHealthCheckPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.PassiveHealthCheck, configurationStateStore, typeof(HealthCheckConstants.PassivePolicy), x => HelpTexts.GetPassiveHealthCheckPolicyText(x));

internal class AvailableDestinationsPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.AvailableDestination, configurationStateStore, typeof(HealthCheckConstants.AvailableDestinations), x => HelpTexts.GetAvailableDestinationsPolicyText(x));

internal class AuthorizationPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Authorization, configurationStateStore, typeof(AuthorizationConstants), x => HelpTexts.GetAuthorizationPolicyText(x));

internal class RateLimitingPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.RateLimiter, configurationStateStore, typeof(RateLimitingConstants), x => HelpTexts.GetRateLimitingPolicyText(x));

internal class OutputCachePolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.OutputCache, configurationStateStore, null);

internal class TimeoutPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Timeout, configurationStateStore, typeof(TimeoutPolicyConstants), x => HelpTexts.GetTimeoutPolicyText(x));

internal class CorsPolicyProvider(IStoreReader<CurrentConfigurationProfileState> configurationStateStore)
    : PolicyProvider(PolicyType.Cors, configurationStateStore, typeof(CorsConstants), x => HelpTexts.GetCorsPolicyText(x));
