using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

public class TimeoutPolicyValidator : PolicyValidatorBase
{
    private readonly IOptionsMonitor<RequestTimeoutOptions> _timeoutOptions;

    public TimeoutPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IOptionsMonitor<RequestTimeoutOptions> timeoutOptions)
        : base(PolicyType.Timeout, policyProvider, NOT_REGISTERED_IN_OPTIONS)
    {
        _timeoutOptions = timeoutOptions;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
    {
        if (ConfigOptionExtractor.GetOptions(typeof(TimeoutPolicyConstants)).Any(x => x.ID == policyID))
            return true;

        return _timeoutOptions.CurrentValue.Policies.TryGetValue(policyID, out var timeoutPolicy) && timeoutPolicy is not null;
    }
}
