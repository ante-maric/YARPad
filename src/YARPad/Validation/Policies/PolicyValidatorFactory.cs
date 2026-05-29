using Microsoft.Extensions.DependencyInjection;

namespace CodingCell.YARPad;

public interface IPolicyValidatorFactory
{
    MudValidator<PolicyInfo> GetValidator(PolicyType policyType);
}

public class PolicyValidatorFactory : IPolicyValidatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PolicyValidatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MudValidator<PolicyInfo> GetValidator(PolicyType policyType)
        => policyType switch
        {
            PolicyType.Authorization => _serviceProvider.GetRequiredService<AuthorizationPolicyValidator>(),
            PolicyType.Timeout => _serviceProvider.GetRequiredService<TimeoutPolicyValidator>(),
            PolicyType.RateLimiter => _serviceProvider.GetRequiredService<RateLimiterPolicyValidator>(),
            PolicyType.Cors => _serviceProvider.GetRequiredService<CorsPolicyValidator>(),
            PolicyType.OutputCache => _serviceProvider.GetRequiredService<OutputCachePolicyValidator>(),
            PolicyType.LoadBalancing => _serviceProvider.GetRequiredService<LoadBalancingPolicyValidator>(),
            PolicyType.SessionAffinity => _serviceProvider.GetRequiredService<SessionAffinityPolicyValidator>(),
            PolicyType.SessionAffinityFailure => _serviceProvider.GetRequiredService<SessionAffinityFailurePolicyValidator>(),
            PolicyType.ActiveHealthCheck => _serviceProvider.GetRequiredService<ActiveHealthCheckPolicyValidator>(),
            PolicyType.PassiveHealthCheck => _serviceProvider.GetRequiredService<PassiveHealthCheckPolicyValidator>(),
            PolicyType.AvailableDestination => _serviceProvider.GetRequiredService<AvailableDestinationPolicyValidator>(),
            _ => throw new ArgumentOutOfRangeException(nameof(policyType), $"Unknown policy type: {policyType}")
        };
}
