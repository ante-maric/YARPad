using AutoMapper;
using FluentValidation;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad;

public class RouteMetadataValidator : MetadataValidator<RouteModel>
{
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public RouteMetadataValidator(IEnumerable<ITransformProvider> transformProviders, YarpMetadataValidator yarpMetadataValidator, IMapper mapper, IServiceProvider serviceProvider)
        : base(transformProviders, yarpMetadataValidator)
    {
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    protected override bool MustBeValidAgaintsTransformProviders(RouteModel parent, List<YarpMetadata> metadata, ValidationContext<List<YarpMetadata>> context)
    {
        var transformValidationContext = new TransformRouteValidationContext
        {
            Route = _mapper.Map<RouteConfig>(parent),
            Services = _serviceProvider
        };

        var values = parent.Metadata.ToDictionary(p => p.Key, p => p.Value);

        foreach (var transformProvider in _transformProviders)
        {
            transformProvider.ValidateRoute(transformValidationContext);
        }

        foreach (var error in transformValidationContext.Errors)
            context.AddFailure(error.Message);

        return transformValidationContext.Errors.Count == 0;
    }
}