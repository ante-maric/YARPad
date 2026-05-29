using FluentValidation;

namespace CodingCell.YARPad;

public abstract class PolicyValidator<T> : MudValidator<T>
{
    private readonly IPolicyValidatorFactory _policyValidatorFactory;

    protected PolicyValidator(IPolicyValidatorFactory policyValidatorFactory)
    {
        _policyValidatorFactory = policyValidatorFactory;
    }

    protected async Task ValidatePolicyAsync(string? policyID, ValidationContext<T> context, PolicyType policyType, CancellationToken token)
    {
        if (policyID == null)
            return;

        var policy = new PolicyInfo { ID = policyID, Name = policyID.HumanizeTitle() };
        var validator = _policyValidatorFactory.GetValidator(policyType);
        var result = await validator.ValidateAsync(policy, token);

        foreach (var failure in result.Errors)
            context.AddFailure(failure.ErrorMessage);
    }
}