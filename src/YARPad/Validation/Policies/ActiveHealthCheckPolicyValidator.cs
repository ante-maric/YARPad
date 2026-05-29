using Yarp.ReverseProxy.Health;

namespace CodingCell.YARPad;

public class ActiveHealthCheckPolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<IActiveHealthCheckPolicy> _aciveHealthCheckPolicies;

    public ActiveHealthCheckPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<IActiveHealthCheckPolicy> aciveHealthCheckPolicies)
        : base(PolicyType.ActiveHealthCheck, policyProvider)
    {
        _aciveHealthCheckPolicies = aciveHealthCheckPolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _aciveHealthCheckPolicies.Any(x => x.Name == policyID);
}
