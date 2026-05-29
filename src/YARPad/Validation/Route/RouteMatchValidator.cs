using FluentValidation;

namespace CodingCell.YARPad;

public class RouteMatchValidator : MudValidator<RouteMatchModel>
{
    public RouteMatchValidator(RouteHeaderValidator headerValidator, RouteQueryParameterValidator queryValidator)
    {
        RuleFor(x => x.Path)
            .Must(path => string.IsNullOrEmpty(path) || path.StartsWith('/'))
                .WithMessage("Path must start with '/'.");

        RuleFor(x => x.Methods)
            .Must(x => x.All(y => HttpMethod.All.Find(z => z.ID == y) != null))
                .WithMessage("Must be list of valid HTTP methods.");

        When(x => string.IsNullOrEmpty(x.Path) && !(x.Hosts?.Any() ?? false), () =>
        {
            RuleFor(x => x.Path)
                .NotEmpty()
                    .WithMessage("Either Path or Hosts must be specified.");

            RuleFor(x => x.Hosts)
                .NotEmpty()
                    .WithMessage("Either Path or Hosts must be specified.");
        });

        RuleForEach(x => x.Headers)
            .SetValidator(headerValidator);

        RuleForEach(x => x.QueryParameters)
            .SetValidator(queryValidator);
    }
}
