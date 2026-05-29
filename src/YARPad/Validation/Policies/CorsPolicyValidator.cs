using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace CodingCell.YARPad;

public class CorsPolicyValidator : PolicyValidatorBase
{
    private readonly ICorsPolicyProvider _corsPolicyProvider;

    public CorsPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        ICorsPolicyProvider corsPolicyProvider)
        : base(PolicyType.Cors, policyProvider, NOT_REGISTERED_IN_OPTIONS)
    {
        _corsPolicyProvider = corsPolicyProvider;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
    {
        if (ConfigOptionExtractor.GetOptions(typeof(RateLimitingConstants)).Any(x => x.ID == policyID))
            return true;

        return await _corsPolicyProvider.GetPolicyAsync(new DefaultHttpContext(), policyID) is not null;
    }
}
