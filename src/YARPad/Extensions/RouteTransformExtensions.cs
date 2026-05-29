namespace CodingCell.YARPad;

internal static class RouteTransformExtensions
{
    private static string BoolToString(bool value) => value ? "true" : "false";

    private static void AddIfHasValue(IDictionary<string, string> dictionary, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            dictionary[key] = value;
        }
    }

    private static void AddIfHasValue<T>(IDictionary<string, string> dictionary, string key, T? value)
        where T : struct
    {
        if (value.HasValue)
        {
            dictionary[key] = value.Value.ToString()!;
        }
    }

    extension(RouteTransform transform)
    {
        public IReadOnlyDictionary<string, string> ToDictionary()
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            switch (transform)
            {
                case RequestHeadersCopyTransform requestHeadersCopy:
                    dictionary["RequestHeadersCopy"] = BoolToString(requestHeadersCopy.RequestHeadersCopy);
                    break;
                case RequestHeaderOriginalHostTransform requestHeaderOriginalHost:
                    dictionary["RequestHeaderOriginalHost"] = BoolToString(requestHeaderOriginalHost.RequestHeaderOriginalHost);
                    break;
                case RequestHeaderTransform requestHeader:
                    dictionary["RequestHeader"] = requestHeader.RequestHeader;
                    AddIfHasValue(dictionary, "Set", requestHeader.Set);
                    AddIfHasValue(dictionary, "Append", requestHeader.Append);
                    if (requestHeader.Remove)
                    {
                        dictionary["Remove"] = BoolToString(requestHeader.Remove);
                    }
                    break;
                case PathRemovePrefixTransform pathRemovePrefix:
                    dictionary["PathRemovePrefix"] = pathRemovePrefix.PathRemovePrefix;
                    break;
                case PathSetTransform pathSet:
                    dictionary["PathSet"] = pathSet.PathSet;
                    break;
                case PathPrefixTransform pathPrefix:
                    dictionary["PathPrefix"] = pathPrefix.PathPrefix;
                    break;
                case QueryRouteParameterTransform queryRouteParameter:
                    dictionary["QueryRouteParameter"] = queryRouteParameter.QueryRouteParameter;
                    AddIfHasValue(dictionary, "Set", queryRouteParameter.Set);
                    AddIfHasValue(dictionary, "Append", queryRouteParameter.Append);
                    break;
                case PathPatternTransform pathPattern:
                    dictionary["PathPattern"] = pathPattern.PathPattern;
                    break;
                case QueryValueParameterTransform queryValueParameter:
                    dictionary["QueryValueParameter"] = queryValueParameter.QueryValueParameter;
                    AddIfHasValue(dictionary, "Set", queryValueParameter.Set);
                    AddIfHasValue(dictionary, "Append", queryValueParameter.Append);
                    break;
                case QueryRemoveParameterTransform queryRemoveParameter:
                    dictionary["QueryRemoveParameter"] = queryRemoveParameter.QueryRemoveParameter;
                    break;
                case HttpMethodChangeTransform httpMethodChange:
                    dictionary["HttpMethodChange"] = httpMethodChange.HttpMethodChange;
                    AddIfHasValue(dictionary, "Set", httpMethodChange.Set);
                    break;
                case RequestHeaderRouteValueTransform requestHeaderRouteValue:
                    dictionary["RequestHeaderRouteValue"] = requestHeaderRouteValue.RequestHeaderRouteValue;
                    AddIfHasValue(dictionary, "Set", requestHeaderRouteValue.Set);
                    AddIfHasValue(dictionary, "Append", requestHeaderRouteValue.Append);
                    break;
                case RequestHeaderRemoveTransform requestHeaderRemove:
                    dictionary["RequestHeaderRemove"] = requestHeaderRemove.RequestHeaderRemove;
                    break;
                case RequestHeadersAllowedTransform requestHeadersAllowed:
                    AddIfHasValue(dictionary, "RequestHeadersAllowed", string.Join(';', requestHeadersAllowed.AllowedHeaders));
                    break;
                case XForwardedTransform xForwarded:
                    dictionary["X-Forwarded"] = xForwarded.XForwarded.ToString();
                    AddIfHasValue(dictionary, "For", xForwarded.For);
                    AddIfHasValue(dictionary, "Proto", xForwarded.Proto);
                    AddIfHasValue(dictionary, "Host", xForwarded.Host);
                    AddIfHasValue(dictionary, "Prefix", xForwarded.Prefix);
                    AddIfHasValue(dictionary, "HeaderPrefix", xForwarded.HeaderPrefix);
                    break;
                case ForwardedTransform forwarded:
                    dictionary["Forwarded"] = string.Join(',', forwarded.Forwarded.Select(node => node.ToString().ToLowerInvariant()));
                    AddIfHasValue(dictionary, "ForFormat", forwarded.ForFormat);
                    AddIfHasValue(dictionary, "ByFormat", forwarded.ByFormat);
                    AddIfHasValue(dictionary, "Action", forwarded.Action);
                    break;
                case ClientCertTransform clientCert:
                    dictionary["ClientCert"] = clientCert.ClientCert;
                    break;
                case ResponseHeadersCopyTransform responseHeadersCopy:
                    dictionary["ResponseHeadersCopy"] = BoolToString(responseHeadersCopy.ResponseHeadersCopy);
                    break;
                case ResponseHeaderTransform responseHeader:
                    dictionary["ResponseHeader"] = responseHeader.ResponseHeader;
                    AddIfHasValue(dictionary, "Set", responseHeader.Set);
                    AddIfHasValue(dictionary, "Append", responseHeader.Append);
                    AddIfHasValue(dictionary, "When", responseHeader.When);
                    break;
                case ResponseHeaderRemoveTransform responseHeaderRemove:
                    dictionary["ResponseHeaderRemove"] = responseHeaderRemove.ResponseHeaderRemove;
                    AddIfHasValue(dictionary, "When", responseHeaderRemove.When);
                    break;
                case ResponseHeadersAllowedTransform responseHeadersAllowed:
                    AddIfHasValue(dictionary, "ResponseHeadersAllowed", string.Join(';', responseHeadersAllowed.AllowedHeaders));
                    break;
                case ResponseTrailersCopyTransform responseTrailersCopy:
                    dictionary["ResponseTrailersCopy"] = BoolToString(responseTrailersCopy.ResponseTrailersCopy);
                    break;
                case ResponseTrailerTransform responseTrailer:
                    dictionary["ResponseTrailer"] = responseTrailer.ResponseTrailer;
                    AddIfHasValue(dictionary, "Set", responseTrailer.Set);
                    AddIfHasValue(dictionary, "Append", responseTrailer.Append);
                    AddIfHasValue(dictionary, "When", responseTrailer.When);
                    break;
                case ResponseTrailerRemoveTransform responseTrailerRemove:
                    dictionary["ResponseTrailerRemove"] = responseTrailerRemove.ResponseTrailerRemove;
                    AddIfHasValue(dictionary, "When", responseTrailerRemove.When);
                    break;
                case ResponseTrailersAllowedTransform responseTrailersAllowed:
                    AddIfHasValue(dictionary, "ResponseTrailersAllowed", string.Join(';', responseTrailersAllowed.AllowedTrailers));
                    break;
                case CustomTransform customTransform:
                    dictionary[customTransform.CustomTransformType] = string.Empty;
                    foreach (var parameter in customTransform.Parameters)
                    {
                        if (!string.IsNullOrWhiteSpace(parameter.Key))
                        {
                            dictionary[parameter.Key] = parameter.Value ?? string.Empty;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unsupported route transform type '{transform.TransformType}'.");
            }

            return dictionary;
        }
    }
}