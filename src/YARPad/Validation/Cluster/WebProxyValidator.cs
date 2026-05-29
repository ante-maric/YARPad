using FluentValidation;

namespace CodingCell.YARPad;

public class WebProxyValidator : MudValidator<WebProxyModel>
{
    public WebProxyValidator()
    {
        RuleFor(x => x.Address)
            .Must(address =>
            {
                try
                {
                    new Uri(address!, UriKind.Absolute);
                    return true;
                }
                catch
                {
                    return false;
                }
            })
                .When(x => x.Address != null)
                .WithMessage("Web proxy address must be a valid absolute URI.");
    }
}
