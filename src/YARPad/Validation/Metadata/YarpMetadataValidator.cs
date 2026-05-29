using FluentValidation;

namespace CodingCell.YARPad;

public class YarpMetadataValidator : MudValidator<YarpMetadata>
{
    public YarpMetadataValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
                .WithMessage("Metadata key cannot be empty.")
            .Must(KeyMustBeUnique)
                .When((metadata, context) => context.RootContextData.ContainsKey(ValidatorContext.Metadata.IS_EDITING))
                .WithMessage("Metadata key must be unique.");
    }

    private bool KeyMustBeUnique(YarpMetadata metadata, string metadataKey, ValidationContext<YarpMetadata> context)
    {
        var originalMetadataKey = context.RootContextData.TryGetValue(ValidatorContext.Metadata.ORIGINAL_KEY, out var value) ? value as string : null;
        if (!context.RootContextData.TryGetValue(ValidatorContext.Metadata.LIST, out var metadataValue) || metadataValue is not List<YarpMetadata> metadataList)
            return false;

        return metadataList.TrueForAll(x => x.Key == originalMetadataKey || x.Key != metadataKey);
    }
}
