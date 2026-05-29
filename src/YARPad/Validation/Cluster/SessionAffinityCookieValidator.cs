using FluentValidation;

namespace CodingCell.YARPad;

public class SessionAffinityCookieValidator : MudValidator<SessionAffinityCookieModel>
{
    public SessionAffinityCookieValidator()
    {
        RuleFor(x => x.Domain)
            .Matches(RegexPatterns.DOMAIN)
                .When(x => x.Domain != null)
                .WithMessage("Domain must be a valid domain name.");

        RuleFor(x => x.SecurePolicy)
            .Must(policy => Enum.IsDefined(policy!.Value))
                .When(x => x.SecurePolicy != null)
                .WithMessage("Secure policy must be a defined enum value.");

        RuleFor(x => x.SameSite)
            .Must(sameSite => Enum.IsDefined(sameSite!.Value))
                .When(x => x.SameSite != null)
                .WithMessage("SameSite policy must be a defined enum value.");
    }
}
