using FluentValidation;

namespace CodingCell.YARPad;

public class SessionAffinityValidator : PolicyValidator<SessionAffinityModel>
{
    public SessionAffinityValidator(IPolicyValidatorFactory policyValidatorFactory, SessionAffinityCookieValidator cookieValidator)
        : base(policyValidatorFactory)
    {
        RuleFor(x => x.AffinityKeyName)
            .NotEmpty()
                .When(x => x.Policy != null)
                .WithMessage("Affinity key name cannot be empty.")
            .Matches(RegexPatterns.HEADER_COOKIE_NAME)
                .When(x => x.Policy != null)
                .WithMessage("Affinity key name must be a valid header or cookie name.");

        RuleFor(x => x.Policy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.SessionAffinity, token))
                .When(x => x.Policy != null);

        RuleFor(x => x.FailurePolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.SessionAffinityFailure, token))
                .When(x => x.FailurePolicy != null);

        RuleFor(x => x.Cookie)
            .SetValidator(cookieValidator);
    }
}
