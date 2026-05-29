using FluentValidation;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public class RouteHeaderValidator : MudValidator<RouteHeaderModel>
{
    public RouteHeaderValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Header Name cannot be empty.");

        RuleFor(x => x.Values)
            .NotEmpty()
                .When(x => x.Mode is HeaderMatchMode.ExactHeader or HeaderMatchMode.HeaderPrefix or HeaderMatchMode.Contains or HeaderMatchMode.NotContains)
                .WithMessage("Values cannot be empty for the selected mode.")
            .Empty()
                .When(x => x.Mode is HeaderMatchMode.Exists or HeaderMatchMode.NotExists)
                .WithMessage("Values must be empty for the selected mode.");
    }
}
