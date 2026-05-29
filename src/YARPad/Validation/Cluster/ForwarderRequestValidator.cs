using FluentValidation;

namespace CodingCell.YARPad;

public class ForwarderRequestValidator : MudValidator<ForwarderRequestModel>
{    public ForwarderRequestValidator()
    {
        RuleFor(x => x.VersionPolicy)
            .Must(policy => Enum.IsDefined(policy!.Value))
                .When(x => x.VersionPolicy != null)
                .WithMessage("Version policy must be a defined enum value.");
    }
}