using AutoMapper;
using FluentValidation;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad;

public class ClusterMetadataValidator : MetadataValidator<ClusterModel>
{
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public ClusterMetadataValidator(IEnumerable<ITransformProvider> transformProviders, YarpMetadataValidator yarpMetadataValidator, IMapper mapper, IServiceProvider serviceProvider) 
        : base(transformProviders, yarpMetadataValidator)
    {
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    protected override void ValidateAgainstTransformProviders(ClusterModel parent, List<YarpMetadata> metadata, ValidationContext<List<YarpMetadata>> context)
    {
        var transformValidationContext = new TransformClusterValidationContext
        {
            Cluster = _mapper.Map<ClusterConfig>(parent),
            Services = _serviceProvider
        };

        foreach (var transformProvider in _transformProviders)
            transformProvider.ValidateCluster(transformValidationContext);
        
        foreach (var error in transformValidationContext.Errors)
            context.AddFailure(error.Message);
    }
}
