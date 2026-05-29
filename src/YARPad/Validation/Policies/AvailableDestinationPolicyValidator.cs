using Yarp.ReverseProxy.Health;

namespace CodingCell.YARPad;

public class AvailableDestinationPolicyValidator : PolicyValidatorBase
{
    private readonly IEnumerable<IAvailableDestinationsPolicy> _availableDestinationPolicies;

    public AvailableDestinationPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IEnumerable<IAvailableDestinationsPolicy> availableDestinationPolicies)
        : base(PolicyType.AvailableDestination, policyProvider)
    {
        _availableDestinationPolicies = availableDestinationPolicies;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => _availableDestinationPolicies.Any(x => x.Name == policyID);
}