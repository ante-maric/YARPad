using FluentValidation;

namespace CodingCell.YARPad;

public abstract class PolicyValidatorBase : MudValidator<PolicyInfo>
{
    protected const string NOT_REGISTERED_IN_DI_CONTAINER = "Policy not registered in DI container.";
    protected const string NOT_REGISTERED_IN_OPTIONS = "Policy not registered in {0} options.";

    private readonly IUnifiedPolicyProvider _policyProvider;
    private readonly string _notRegisteredErrorMessage;
    private readonly PolicyType _policyType;

    protected PolicyValidatorBase(
        PolicyType policyType,
        IUnifiedPolicyProvider policyProvider,
        string notRegisteredErrorMessage = NOT_REGISTERED_IN_DI_CONTAINER)
    {
        _policyProvider = policyProvider;
        _notRegisteredErrorMessage = notRegisteredErrorMessage;
        _policyType = policyType;

        if (_notRegisteredErrorMessage == NOT_REGISTERED_IN_OPTIONS)
            _notRegisteredErrorMessage = string.Format(_notRegisteredErrorMessage, _policyType);

        RuleFor(x => x.ID)
            .NotEmpty()
                .WithMessage("Policy ID cannot be empty.")
            .MustAsync(IDMustBeUniqueAsync)
                .When((policy, ctx) => ctx.RootContextData.ContainsKey(ValidatorContext.Policy.IS_EDITING))
                .WithMessage("Policy ID must be unique.")
            .MustAsync((policy, id, context, cancellationToken) => PolicyMustBeRegistered(policy, id, cancellationToken))
                .WithMessage(_notRegisteredErrorMessage);
    }

    protected abstract Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken);

    private async Task<bool> IDMustBeUniqueAsync(PolicyInfo policy, string policyID, ValidationContext<PolicyInfo> context, CancellationToken cancellationToken)
    {
        var originalPolicyID = context.RootContextData.TryGetValue(ValidatorContext.Policy.ORIGINAL_ID, out var value) ? value as string : null;

        var policies = await _policyProvider.GetPoliciesAsync(_policyType, cancellationToken);

        return policies.TrueForAll(x => x.ID == originalPolicyID || x.ID != policyID);
    }
}
