using AutoMapper;
using FluentValidation;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad;

public class RouteTransformValidator : MudValidator<RouteTransform>
{
    public RouteTransformValidator(CustomTransformValidator custromTransformValidator)
    {
        RuleFor(x => x)
            .SetInheritanceValidator(v =>
            {
                v.Add(new RequestHeadersCopyTransformValidator());
                v.Add(new RequestHeaderOriginalHostTransformValidator());
                v.Add(new RequestHeaderTransformValidator());
                v.Add(new PathRemovePrefixTransformValidator());
                v.Add(new PathSetTransformValidator());
                v.Add(new PathPrefixTransformValidator());
                v.Add(new QueryRouteParameterTransformValidator());
                v.Add(new PathPatternTransformValidator());
                v.Add(new QueryValueParameterTransformValidator());
                v.Add(new QueryRemoveParameterTransformValidator());
                v.Add(new HttpMethodChangeTransformValidator());
                v.Add(new RequestHeaderRouteValueTransformValidator());
                v.Add(new RequestHeaderRemoveTransformValidator());
                v.Add(new RequestHeadersAllowedTransformValidator());
                v.Add(new XForwardedTransformValidator());
                v.Add(new ForwardedTransformValidator());
                v.Add(new ClientCertTransformValidator());
                v.Add(new ResponseHeadersCopyTransformValidator());
                v.Add(new ResponseHeaderTransformValidator());
                v.Add(new ResponseHeaderRemoveTransformValidator());
                v.Add(new ResponseHeadersAllowedTransformValidator());
                v.Add(new ResponseTrailersCopyTransformValidator());
                v.Add(new ResponseTrailerTransformValidator());
                v.Add(new ResponseTrailerRemoveTransformValidator());
                v.Add(new ResponseTrailersAllowedTransformValidator());
                v.Add(custromTransformValidator);
            });
    }
}

public class RequestHeadersCopyTransformValidator : MudValidator<RequestHeadersCopyTransform>
{
    public RequestHeadersCopyTransformValidator()
    {
        RuleFor(x => x.RequestHeadersCopy)
            .NotNull()
                .WithMessage("RequestHeadersCopy is required.");
    }
}

public class RequestHeaderOriginalHostTransformValidator : MudValidator<RequestHeaderOriginalHostTransform>
{
    public RequestHeaderOriginalHostTransformValidator()
    {
        RuleFor(x => x.RequestHeaderOriginalHost)
            .NotNull()
                .WithMessage("RequestHeaderOriginalHost is required.");
    }
}

public class RequestHeaderTransformValidator : MudValidator<RequestHeaderTransform>
{
    public RequestHeaderTransformValidator()
    {
        RuleFor(x => x.RequestHeader)
            .NotEmpty()
                .WithMessage("Request header name is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => !x.Remove && string.IsNullOrEmpty(x.Append))
                .WithMessage("Header value is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => !x.Remove && string.IsNullOrEmpty(x.Set))
                .WithMessage("Header value is required.");
    }
}

public class PathRemovePrefixTransformValidator : MudValidator<PathRemovePrefixTransform>
{
    public PathRemovePrefixTransformValidator()
    {
        RuleFor(x => x.PathRemovePrefix)
            .NotEmpty()
                .WithMessage("Path Prefix is required.");
    }
}

public class PathSetTransformValidator : MudValidator<PathSetTransform>
{
    public PathSetTransformValidator()
    {
        RuleFor(x => x.PathSet)
            .NotEmpty()
                .WithMessage("Replacement Path is required.");
    }
}

public class PathPrefixTransformValidator : MudValidator<PathPrefixTransform>
{
    public PathPrefixTransformValidator()
    {
        RuleFor(x => x.PathPrefix)
            .NotEmpty()
                .WithMessage("Path Prefix is required.");
    }
}

public class QueryRouteParameterTransformValidator : MudValidator<QueryRouteParameterTransform>
{
    public QueryRouteParameterTransformValidator()
    {
        RuleFor(x => x.QueryRouteParameter)
            .NotEmpty()
                .WithMessage("Query Parameter is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Append))
                .WithMessage("Route Parameter is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Set))
                .WithMessage("Route Parameter is required.");
    }
}

public class PathPatternTransformValidator : MudValidator<PathPatternTransform>
{
    public PathPatternTransformValidator()
    {
        RuleFor(x => x.PathPattern)
            .NotEmpty()
                .WithMessage("Path Pattern is required.")
            .Must(x => x.StartsWith('/'))
                .WithMessage("Path Pattern must start with '/'.");
    }
}

public class QueryValueParameterTransformValidator : MudValidator<QueryValueParameterTransform>
{
    public QueryValueParameterTransformValidator()
    {
        RuleFor(x => x.QueryValueParameter)
            .NotEmpty()
                .WithMessage("QueryValueParameter is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Append))
                .WithMessage("Query Parameter value is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Set))
                .WithMessage("Query Parameter value is required.");
    }
}

public class QueryRemoveParameterTransformValidator : MudValidator<QueryRemoveParameterTransform>
{
    public QueryRemoveParameterTransformValidator()
    {
        RuleFor(x => x.QueryRemoveParameter)
            .NotEmpty()
                .WithMessage("Query Parameter is required.");
    }
}

public class HttpMethodChangeTransformValidator : MudValidator<HttpMethodChangeTransform>
{
    public HttpMethodChangeTransformValidator()
    {
        RuleFor(x => x.HttpMethodChange)
            .NotEmpty()
                .WithMessage("Source Method is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .WithMessage("Destination Method is required.");
    }
}

public class RequestHeaderRouteValueTransformValidator : MudValidator<RequestHeaderRouteValueTransform>
{
    public RequestHeaderRouteValueTransformValidator()
    {
        RuleFor(x => x.RequestHeaderRouteValue)
            .NotEmpty()
                .WithMessage("Request header name is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Append))
                .WithMessage("Route value is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Set))
                .WithMessage("Route value is required.");
    }
}

public class RequestHeaderRemoveTransformValidator : MudValidator<RequestHeaderRemoveTransform>
{
    public RequestHeaderRemoveTransformValidator()
    {
        RuleFor(x => x.RequestHeaderRemove)
            .NotEmpty()
                .WithMessage("RequestHeaderRemove is required.");
    }
}

public class RequestHeadersAllowedTransformValidator : MudValidator<RequestHeadersAllowedTransform>
{
    public RequestHeadersAllowedTransformValidator()
    {
        RuleFor(x => x.AllowedHeaders)
            .NotEmpty()
                .WithMessage("At least one allowed header is required.");

        RuleForEach(x => x.AllowedHeaders)
            .NotEmpty()
                .Matches(RegexPatterns.HEADER_COOKIE_NAME)
                .WithMessage("Allowed header contains invalid characters.");
    }
}

public class XForwardedTransformValidator : MudValidator<XForwardedTransform>
{
    public XForwardedTransformValidator()
    {
        RuleFor(x => x.XForwarded)
            .Must(action => Enum.IsDefined(action))
                .WithMessage("Default action must be a defined enum value.");
    }
}

public class ForwardedTransformValidator : MudValidator<ForwardedTransform>
{
    public ForwardedTransformValidator()
    {
        RuleFor(x => x.Forwarded)
            .NotEmpty()
            .WithMessage("Forwarded must include at least one of For, By, Proto, or Host.");

        RuleFor(x => x.ForFormat)
            .Must(format => Enum.IsDefined(format!.Value))
                .When(x => x.ForFormat != null)
                .WithMessage("For Format must be a defined enum value.");

        RuleFor(x => x.ByFormat)
            .Must(format => Enum.IsDefined(format!.Value))
                .When(x => x.ByFormat != null)
                .WithMessage("By Format must be a defined enum value.");

        RuleFor(x => x.Action)
            .Must(action => Enum.IsDefined(action!.Value))
                .When(x => x.Action != null)
                .WithMessage("Action must be a defined enum value.");
    }
}

public class ClientCertTransformValidator : MudValidator<ClientCertTransform>
{
    public ClientCertTransformValidator()
    {
        RuleFor(x => x.ClientCert)
            .NotEmpty()
            .WithMessage("Header name is required.");
    }
}

public class ResponseHeadersCopyTransformValidator : MudValidator<ResponseHeadersCopyTransform>
{
    public ResponseHeadersCopyTransformValidator()
    {
        RuleFor(x => x.ResponseHeadersCopy)
            .NotNull()
                .WithMessage("ResponseHeadersCopy is required.");
    }
}

public class ResponseHeaderTransformValidator : MudValidator<ResponseHeaderTransform>
{
    public ResponseHeaderTransformValidator()
    {
        RuleFor(x => x.ResponseHeader)
            .NotEmpty()
            .WithMessage("Response Header is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Append))
                .WithMessage("Header value is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Set))
                .WithMessage("Header value is required.");
    }
}

public class ResponseHeaderRemoveTransformValidator : MudValidator<ResponseHeaderRemoveTransform>
{
    public ResponseHeaderRemoveTransformValidator()
    {
        RuleFor(x => x.ResponseHeaderRemove)
            .NotEmpty()
                .WithMessage("Response Header is required.");
    }
}

public class ResponseHeadersAllowedTransformValidator : MudValidator<ResponseHeadersAllowedTransform>
{
    public ResponseHeadersAllowedTransformValidator()
    {
        RuleFor(x => x.AllowedHeaders)
            .NotEmpty()
                .WithMessage("At least one allowed header is required.");

        RuleForEach(x => x.AllowedHeaders)
            .NotEmpty()
                .Matches(RegexPatterns.HEADER_COOKIE_NAME)
                .WithMessage("Allowed header contains invalid characters.");
    }
}

public class ResponseTrailersCopyTransformValidator : MudValidator<ResponseTrailersCopyTransform>
{
    public ResponseTrailersCopyTransformValidator()
    {
        RuleFor(x => x.ResponseTrailersCopy)
            .NotNull()
                .WithMessage("ResponseTrailersCopy is required.");
    }
}

public class ResponseTrailerTransformValidator : MudValidator<ResponseTrailerTransform>
{
    public ResponseTrailerTransformValidator()
    {
        RuleFor(x => x.ResponseTrailer)
            .NotEmpty()
            .WithMessage("Response Trailer is required.");

        RuleFor(x => x.Set)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Append))
                .WithMessage("Trailer value is required.");

        RuleFor(x => x.Append)
            .NotEmpty()
                .When(x => string.IsNullOrEmpty(x.Set))
                    .WithMessage("Trailer value is required.");
    }
}

public class ResponseTrailerRemoveTransformValidator : MudValidator<ResponseTrailerRemoveTransform>
{
    public ResponseTrailerRemoveTransformValidator()
    {
        RuleFor(x => x.ResponseTrailerRemove)
            .NotEmpty()
                .WithMessage("Response Trailer is required.");
    }
}

public class ResponseTrailersAllowedTransformValidator : MudValidator<ResponseTrailersAllowedTransform>
{
    public ResponseTrailersAllowedTransformValidator()
    {
        RuleFor(x => x.AllowedTrailers)
            .NotEmpty()
                .WithMessage("At least one allowed response trailer is required.");

        RuleForEach(x => x.AllowedTrailers)
            .NotEmpty()
                .Matches(RegexPatterns.HEADER_COOKIE_NAME)
                .WithMessage("Allowed response trailer contains invalid characters.");
    }
}

public class CustomTransformValidator : MudValidator<CustomTransform>
{
    private readonly IEnumerable<ITransformFactory> _transformFactories;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public CustomTransformValidator(IEnumerable<ITransformFactory> transformFactories, CustomTransformParameterValidator parameterValidator, IMapper mapper, IServiceProvider serviceProvider)
    {
        _transformFactories = transformFactories;
        _mapper = mapper;
        _serviceProvider = serviceProvider;

        RuleFor(x => x)
            .Must((transform, transform2, context) =>
            {
                if (!context.RootContextData.TryGetValue(ValidatorContext.Route.MODEL, out var routeObj) || routeObj is not RouteModel route)
                    return true;

                return CustomTransformsMustBeRegistered(route, transform, context);
            });

        RuleFor(x => x.Parameters)
            .Must(HaveUniqueKeys)
                .WithMessage("Parameter keys must be unique (case-insensitive).");

        RuleForEach(x => x.Parameters)
            .SetValidator(parameterValidator);
    }

    private static bool HaveUniqueKeys(List<CustomTransformParameter>? parameters)
    {
        if (parameters == null)
            return false;

        var keys = parameters.Select(p => p.Key?.Trim() ?? string.Empty);
        return keys.Distinct().Count() == parameters.Count;
    }

    private bool CustomTransformsMustBeRegistered(RouteModel route, CustomTransform transform, ValidationContext<CustomTransform> context)
    {
        var transformValidationContext = new TransformRouteValidationContext
        {
            Route = _mapper.Map<RouteConfig>(route),
            Services = _serviceProvider
        };

        var transformValues = transform.Parameters.ToDictionary(p => p.Key, p => p.Value);

        if (!_transformFactories.Any(f => f.Validate(transformValidationContext, transformValues!)))
        {
            context.AddFailure($"{transform.TransformType} is not properly registered (ITransformFactory) in DI container.");
            return false;
        }

        var isEditing = context.RootContextData.ContainsKey(ValidatorContext.CustomTransform.IS_EDITING);
        foreach (var error in transformValidationContext.Errors)
        {
            if (isEditing)
                context.AddFailure(error.Message);
            else
                context.AddFailure($"[{transform.TransformType}] {error.Message}");
        }

        return transformValidationContext.Errors.Count == 0;
    }
}

public class CustomTransformParameterValidator : MudValidator<CustomTransformParameter>
{
    public CustomTransformParameterValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
                .WithMessage("Parameter key is required.");
    }
}
