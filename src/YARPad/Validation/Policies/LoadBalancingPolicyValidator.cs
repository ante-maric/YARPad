using Yarp.ReverseProxy.LoadBalancing;

namespace CodingCell.YARPad;

public class LoadBalancingPolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<ILoadBalancingPolicy> _loadBalancingPolicies;

    public LoadBalancingPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<ILoadBalancingPolicy> loadBalancingPolicies)
        : base(PolicyType.LoadBalancing, policyProvider)
    {
        _loadBalancingPolicies = loadBalancingPolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _loadBalancingPolicies.Any(x => x.Name == policyID);
}
