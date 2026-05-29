using FluentValidation;

namespace CodingCell.YARPad;

public class PassiveHealthCheckValidator : PolicyValidator<PassiveHealthCheckModel>
{
    public PassiveHealthCheckValidator(IPolicyValidatorFactory policyValidatorFactory)
        : base(policyValidatorFactory)
    {

        RuleFor(x => x.Policy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.PassiveHealthCheck, token))
                .When(x => x.Policy != null);
    }
}