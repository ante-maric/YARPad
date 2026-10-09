using FluentValidation;

namespace CodingCell.YARPad;

public class DestinationValidator : MudValidator<DestinationModel>
{
    public DestinationValidator()
    {
        RuleFor(x => x.ID)
            .NotEmpty()
                .WithMessage("Destination ID cannot be empty.");

        RuleFor(x => x.ID)
            .Must(IDMustBeUnique)
                .When((dest, context) => context.GetIsEditingDestination())
                .WithMessage("Destination ID must be unique within the cluster.");

        RuleFor(x => x.Address)
            .NotEmpty()
                .WithMessage("Address cannot be empty.")
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
                .WithMessage("Address must be a valid absolute URI.");

        RuleFor(x => x.Health)
            .Must(health => Uri.TryCreate(health, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.Health))
                .WithMessage("Health URI must be a valid absolute URI.");

        RuleFor(x => x.Host)
            .Matches(RegexPatterns.HOST)
                .When(x => !string.IsNullOrEmpty(x.Host))
                .WithMessage("Host must be a valid host name or IP address.");
    }

    private static bool IDMustBeUnique(DestinationModel destination, string destinationID, ValidationContext<DestinationModel> context)
    {
        var originalDestinationID = context.GetOriginalDestinationID();
        var cluster = context.GetClusterModel();
        if (cluster == null)
            return false;

        return cluster.Destinations.TrueForAll(x => x.ID == originalDestinationID || x.ID != destinationID);
    }
}
