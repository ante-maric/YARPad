using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad;

public class SessionAffinityPolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<ISessionAffinityPolicy> _sessionAffinityPolicies;

    public SessionAffinityPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<ISessionAffinityPolicy> sessionAffinityPolicies)
        : base(PolicyType.SessionAffinity, policyProvider)
    {
        _sessionAffinityPolicies = sessionAffinityPolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _sessionAffinityPolicies.Any(x => x.Name == policyID);
}
