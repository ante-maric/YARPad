using Yarp.ReverseProxy.Health;

namespace CodingCell.YARPad;

public class PassiveHealthCheckPolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<IPassiveHealthCheckPolicy> _passiveHealthCheckPolicies;

    public PassiveHealthCheckPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<IPassiveHealthCheckPolicy> passiveHealthCheckPolicies)
        : base(PolicyType.PassiveHealthCheck, policyProvider)
    {
        _passiveHealthCheckPolicies = passiveHealthCheckPolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _passiveHealthCheckPolicies.Any(x => x.Name == policyID);
}
