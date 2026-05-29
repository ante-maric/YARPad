using FluentValidation;
using FluentValidation.Results;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad;

public abstract class MetadataValidator<T> : MudValidator<List<YarpMetadata>>
{
    protected readonly IEnumerable<ITransformProvider> _transformProviders;

    public MetadataValidator(IEnumerable<ITransformProvider> transformProviders, YarpMetadataValidator yarpMetadataValidator)
    {
        _transformProviders = transformProviders;

        RuleFor(x => x)
            .Must((metadata, metadata2, context) =>
            {
                if (!context.RootContextData.TryGetValue(typeof(T).Name, out var parentObj) || parentObj is not T parent)
                    return true;

                return MustBeValidAgaintsTransformProviders(parent, metadata, context);
            })
            .Must(x => HaveUniqueIDs(x, x => x.Key))
                .WithMessage("Metadata must have unique keys (case-sensitive).");


        RuleForEach(x => x)
            .SetValidator(yarpMetadataValidator);
    }

    protected override bool PreValidate(ValidationContext<List<YarpMetadata>> context, ValidationResult result)
    {
        context.RootContextData[ValidatorContext.Metadata.LIST] = context.InstanceToValidate;
        return base.PreValidate(context, result);
    }

    protected abstract bool MustBeValidAgaintsTransformProviders(T parent, List<YarpMetadata> metadata, ValidationContext<List<YarpMetadata>> context);
}
