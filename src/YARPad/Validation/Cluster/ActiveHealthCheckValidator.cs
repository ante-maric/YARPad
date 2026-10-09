using FluentValidation;

namespace CodingCell.YARPad;

public class ActiveHealthCheckValidator : PolicyValidator<ActiveHealthCheckModel>
{
    public ActiveHealthCheckValidator(IPolicyValidatorFactory policyValidatorFactory)
        : base(policyValidatorFactory)
    {
        RuleFor(x => x.Policy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.ActiveHealthCheck, token))
                .When(x => !string.IsNullOrEmpty(x.Policy));

        RuleFor(x => x.Path)
            .Must(path => path!.StartsWith("/"))
                .When(x => !string.IsNullOrEmpty(x.Path))
                .WithMessage("Active health check path must start with '/'.");

        RuleFor(x => x.Query)
            .Must(query => query!.StartsWith("?"))
                .When(x => !string.IsNullOrEmpty(x.Query))
                .WithMessage("Active health check query must start with '?'.");
    }
}
