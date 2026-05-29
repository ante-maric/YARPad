namespace CodingCell.YARPad;

public class RateLimiterPolicyValidator : PolicyValidatorBase
{
    private readonly IYarpRateLimiterPolicyProvider _rateLimiterPolicyProvider;

    public RateLimiterPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IYarpRateLimiterPolicyProvider rateLimiterPolicyProvider)
        : base(PolicyType.RateLimiter, policyProvider, NOT_REGISTERED_IN_OPTIONS)
    {
        _rateLimiterPolicyProvider = rateLimiterPolicyProvider;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
    {
        if (ConfigOptionExtractor.GetOptions(typeof(RateLimitingConstants)).Any(x => x.ID == policyID))
            return true;

        return await _rateLimiterPolicyProvider.GetPolicyAsync(policyID) is not null;
    }
}
