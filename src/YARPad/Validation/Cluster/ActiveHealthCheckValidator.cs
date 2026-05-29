using FluentValidation;

namespace CodingCell.YARPad;

public class ActiveHealthCheckValidator : PolicyValidator<ActiveHealthCheckModel>
{
    public ActiveHealthCheckValidator(IPolicyValidatorFactory policyValidatorFactory)
        : base(policyValidatorFactory)
    {
        RuleFor(x => x.Policy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.ActiveHealthCheck, token))
                .When(x => x.Policy != null);

        RuleFor(x => x.Path)
            .Must(path => path!.StartsWith("/"))
                .When(x => x.Path != null)
                .WithMessage("Active health check path must start with '/'.");

        RuleFor(x => x.Query)
            .Must(query => query!.StartsWith("?"))
                .When(x => x.Query != null)
                .WithMessage("Active health check query must start with '?'.");
    }
}
