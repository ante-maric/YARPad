using FluentValidation;

namespace CodingCell.YARPad;

public class CustomTransformParameterDefinitionValidator : MudValidator<CustomTransformParameterDefinition>
{
    public CustomTransformParameterDefinitionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Parameter name is required.")
            .Must(NameMustBeUnique)
                .When((transform, context) => context.RootContextData.ContainsKey(ValidatorContext.CustomTransform.IS_EDITING_PARAMETER))
                .WithMessage("Parameter name must be unique within the transform.");
    }

    private static bool NameMustBeUnique(CustomTransformParameterDefinition param, string name, ValidationContext<CustomTransformParameterDefinition> context)
    {
        var originalName = context.RootContextData.TryGetValue(ValidatorContext.CustomTransform.ORIGINAL_PARAMETER_NAME, out var value) ? value as string : null;
        if (!context.RootContextData.TryGetValue(ValidatorContext.CustomTransform.MODEL, out var transformObj) || transformObj is not CustomTransformDefinition transform)
            return false;

        return transform.Parameters.TrueForAll(p => p.Name == originalName || p.Name != name);
    }
}
