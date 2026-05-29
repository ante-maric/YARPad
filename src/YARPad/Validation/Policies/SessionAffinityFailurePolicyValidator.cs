using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad;

public class SessionAffinityFailurePolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<IAffinityFailurePolicy> _sessionAffinityFailurePolicies;

    public SessionAffinityFailurePolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<IAffinityFailurePolicy> sessionAffinityFailurePolicies)
        : base(PolicyType.SessionAffinityFailure, policyProvider)
    {
        _sessionAffinityFailurePolicies = sessionAffinityFailurePolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _sessionAffinityFailurePolicies.Any(x => x.Name == policyID);
}
