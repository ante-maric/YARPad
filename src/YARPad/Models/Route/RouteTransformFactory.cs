namespace CodingCell.YARPad;

internal static class RouteTransformFactory
{
    public static RouteTransform Create(string type, List<CustomTransformDefinition> customTransforms)
    {
        if (Enum.TryParse<RouteTransformType>(type, out var transformType))
            return Create(transformType);

        if (customTransforms.Find(x => x.Type == type) is CustomTransformDefinition customTransform)
        {
            return new CustomTransform
            {
                CustomTransformType = type,
                Description = customTransform.Description ?? "Custom transform",
                ParameterDefinitions = customTransform.Parameters,
                Parameters = customTransform.Parameters.ConvertAll(x => new CustomTransformParameter() {  Key = x.Name  })
            };
        }

        throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown transform type.");
    }

    public static RouteTransform Create(RouteTransformType type)
    {
        return type switch
        {
            RouteTransformType.RequestHeadersCopy => new RequestHeadersCopyTransform { RequestHeadersCopy = true },
            RouteTransformType.RequestHeaderOriginalHost => new RequestHeaderOriginalHostTransform { RequestHeaderOriginalHost = true },
            RouteTransformType.RequestHeader => new RequestHeaderTransform { RequestHeader = string.Empty },
            RouteTransformType.PathRemovePrefix => new PathRemovePrefixTransform { PathRemovePrefix = string.Empty },
            RouteTransformType.PathSet => new PathSetTransform { PathSet = string.Empty },
            RouteTransformType.PathPrefix => new PathPrefixTransform { PathPrefix = string.Empty },
            RouteTransformType.QueryRouteParameter => new QueryRouteParameterTransform { QueryRouteParameter = string.Empty },
            RouteTransformType.PathPattern => new PathPatternTransform { PathPattern = "/" },
            RouteTransformType.QueryValueParameter => new QueryValueParameterTransform { QueryValueParameter = string.Empty },
            RouteTransformType.QueryRemoveParameter => new QueryRemoveParameterTransform { QueryRemoveParameter = string.Empty },
            RouteTransformType.HttpMethodChange => new HttpMethodChangeTransform { HttpMethodChange = string.Empty, Set = string.Empty },
            RouteTransformType.RequestHeaderRouteValue => new RequestHeaderRouteValueTransform { RequestHeaderRouteValue = string.Empty },
            RouteTransformType.RequestHeaderRemove => new RequestHeaderRemoveTransform { RequestHeaderRemove = string.Empty },
            RouteTransformType.RequestHeadersAllowed => new RequestHeadersAllowedTransform(),
            RouteTransformType.XForwarded => new XForwardedTransform { XForwarded = ForwardedTransformAction.Set },
            RouteTransformType.Forwarded => new ForwardedTransform(),
            RouteTransformType.ClientCert => new ClientCertTransform { ClientCert = null! },
            RouteTransformType.ResponseHeadersCopy => new ResponseHeadersCopyTransform { ResponseHeadersCopy = true },
            RouteTransformType.ResponseHeader => new ResponseHeaderTransform { ResponseHeader = string.Empty },
            RouteTransformType.ResponseHeaderRemove => new ResponseHeaderRemoveTransform { ResponseHeaderRemove = string.Empty },
            RouteTransformType.ResponseHeadersAllowed => new ResponseHeadersAllowedTransform(),
            RouteTransformType.ResponseTrailersCopy => new ResponseTrailersCopyTransform { ResponseTrailersCopy = true },
            RouteTransformType.ResponseTrailer => new ResponseTrailerTransform { ResponseTrailer = string.Empty },
            RouteTransformType.ResponseTrailerRemove => new ResponseTrailerRemoveTransform { ResponseTrailerRemove = string.Empty },
            RouteTransformType.ResponseTrailersAllowed => new ResponseTrailersAllowedTransform(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown transform type.")
        };
    }

    public static RouteTransform Clone(RouteTransform transform) => transform switch
    {
        RequestHeadersAllowedTransform h => h with { AllowedHeaders = new(h.AllowedHeaders) },
        ResponseHeadersAllowedTransform h => h with { AllowedHeaders = new(h.AllowedHeaders) },
        ResponseTrailersAllowedTransform t => t with { AllowedTrailers = new(t.AllowedTrailers) },
        ForwardedTransform f => f with { Forwarded = new(f.Forwarded) },
        CustomTransform c => c with
        {
            ParameterDefinitions = (c.ParameterDefinitions ?? []).Select(p => new CustomTransformParameterDefinition
            {
                Name = p.Name,
                Description = p.Description
            }).ToList(),
            Parameters = (c.Parameters ?? []).Select(p => new CustomTransformParameter
            {
                Key = p.Key,
                Value = p.Value
            }).ToList()
        },
        _ => transform with { }
    };
}
