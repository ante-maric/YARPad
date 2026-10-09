using FluentValidation;

namespace CodingCell.YARPad;

public class WebProxyValidator : MudValidator<WebProxyModel>
{
    public WebProxyValidator()
    {
        RuleFor(x => x.Address)
            .Must(address => Uri.TryCreate(address, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.Address))
                .WithMessage("Web proxy address must be a valid absolute URI.");
    }
}
