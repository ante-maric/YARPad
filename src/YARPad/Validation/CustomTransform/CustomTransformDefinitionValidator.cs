using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.YARPad;

public class CustomTransformDefinitionValidator : MudValidator<CustomTransformDefinition>
{
    private readonly IStoreReader<ConfigurationProfileState> _configurationProfileStore;

    public CustomTransformDefinitionValidator(
        IStoreReader<ConfigurationProfileState> configurationProfileStore, 
        CustomTransformParameterDefinitionValidator parameterValidator)
    {
        _configurationProfileStore = configurationProfileStore;

        RuleFor(x => x.Type)
            .NotEmpty()
                .WithMessage("Transform type is required.")
            .Must(TypeMustBeUnique)
                .WithMessage("Transform type must be unique.")
            .Must(type => !Enum.TryParse<RouteTransformType>(type, true, out _))
                .WithMessage("Transform type must not match a built-in route transform type.");

        RuleForEach(x => x.Parameters)
            .SetValidator(parameterValidator);

        RuleFor(x => x.Parameters)
            .Must(x => HaveUniqueIDs(x, x => x.Name))
                .WithMessage("Parameter names must be unique (case-sensitive).");
    }

    protected override bool PreValidate(ValidationContext<CustomTransformDefinition> context, ValidationResult result)
    {
        context.RootContextData[ValidatorContext.CustomTransform.MODEL] = context.InstanceToValidate;

        return base.PreValidate(context, result);
    }

    private bool TypeMustBeUnique(CustomTransformDefinition definition, string type, ValidationContext<CustomTransformDefinition> context)
    {
        var configuration = context.GetConfiguration(_configurationProfileStore);
        var originalType = context.RootContextData.TryGetValue(ValidatorContext.CustomTransform.ORIGINAL_TYPE, out var value) ? value as string : null;

        return configuration?.CustomTransforms.TrueForAll(x => x.Type == originalType || x.Type != type) == true;
    }
}
