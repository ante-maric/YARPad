using FluentValidation;

namespace CodingCell.YARPad;

public class ConfigurationProfileValidator : MudValidator<ConfigurationProfile>
{
    private readonly IYARPadConfigurationProvider _configurationProvider;

    public ConfigurationProfileValidator(IYARPadConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Configuration name cannot be empty.")
            .MustAsync(NameMustBeUniqueAsync)
                .WithMessage("Configuration name must be unique.");

    }
    private async Task<bool> NameMustBeUniqueAsync(ConfigurationProfile model, string name, ValidationContext<ConfigurationProfile> context, CancellationToken cancellation)
    {
        var configurations = await _configurationProvider.GetConfigurationsAsync();

        return configurations.TrueForAll(x => x.ID == model.ID || x.Name != name);
    }
}
