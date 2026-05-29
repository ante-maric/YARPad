using FluentValidation;

namespace CodingCell.YARPad;

public class HealthCheckValidator : PolicyValidator<HealthCheckModel>
{
    public HealthCheckValidator(IPolicyValidatorFactory policyValidatorFactory, ActiveHealthCheckValidator activeValidator, PassiveHealthCheckValidator passiveValidator)
        : base(policyValidatorFactory)
    {
        RuleFor(x => x.AvailableDestinationsPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.AvailableDestination, token))
                .When(x => x.AvailableDestinationsPolicy != null);
        
        RuleFor(x => x.Active)
            .SetValidator(activeValidator);

        RuleFor(x => x.Passive)
            .SetValidator(passiveValidator);
    }
}
