using FluentValidation;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public class RouteQueryParameterValidator : MudValidator<RouteQueryParameterModel>
{
    public RouteQueryParameterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Query Parameter Name cannot be empty.");

        RuleFor(x => x.Values)
            .NotEmpty()
                .When(x => x.Mode is QueryParameterMatchMode.Exact or QueryParameterMatchMode.Prefix or QueryParameterMatchMode.Contains or QueryParameterMatchMode.NotContains)
                .WithMessage("Values cannot be empty for the selected mode.")
            .Empty()
                .When(x => x.Mode is QueryParameterMatchMode.Exists)
                .WithMessage("Values must be empty for the selected mode.");
    }
}