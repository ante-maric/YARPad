using Microsoft.AspNetCore.Authorization;

namespace CodingCell.YARPad;

public class AuthorizationPolicyValidator : PolicyValidatorBase
{
    private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider;

    public AuthorizationPolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IAuthorizationPolicyProvider authorizationPolicyProvider)
        : base(PolicyType.Authorization, policyProvider, NOT_REGISTERED_IN_OPTIONS)
    {
        _authorizationPolicyProvider = authorizationPolicyProvider;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
    {
        if (ConfigOptionExtractor.GetOptions(typeof(AuthorizationConstants)).Any(x => x.ID == policyID))
            return true;

        return await _authorizationPolicyProvider.GetPolicyAsync(policyID) is not null;
    }
}
