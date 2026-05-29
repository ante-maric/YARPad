namespace CodingCell.YARPad;

internal static class RouteTransformTypeExtensions
{
    extension(RouteTransformType type)
    {
        public string GetDescription()
        {
            return type switch
            {
                RouteTransformType.RequestHeadersCopy => Tooltips.Transform_RequestHeadersCopy,
                RouteTransformType.RequestHeaderOriginalHost => Tooltips.Transform_RequestHeaderOriginalHost,
                RouteTransformType.RequestHeader => Tooltips.Transform_RequestHeader,
                RouteTransformType.PathRemovePrefix => Tooltips.Transform_PathRemovePrefix,
                RouteTransformType.PathSet => Tooltips.Transform_PathSet,
                RouteTransformType.PathPrefix => Tooltips.Transform_PathPrefix,
                RouteTransformType.QueryRouteParameter => Tooltips.Transform_QueryRouteParameter,
                RouteTransformType.PathPattern => Tooltips.Transform_PathPattern,
                RouteTransformType.QueryValueParameter => Tooltips.Transform_QueryValueParameter,
                RouteTransformType.QueryRemoveParameter => Tooltips.Transform_QueryRemoveParameter,
                RouteTransformType.HttpMethodChange => Tooltips.Transform_HttpMethodChange,
                RouteTransformType.RequestHeaderRouteValue => Tooltips.Transform_RequestHeaderRouteValue,
                RouteTransformType.RequestHeaderRemove => Tooltips.Transform_RequestHeaderRemove,
                RouteTransformType.RequestHeadersAllowed => Tooltips.Transform_RequestHeadersAllowed,
                RouteTransformType.XForwarded => Tooltips.Transform_XForwarded,
                RouteTransformType.Forwarded => Tooltips.Transform_Forwarded,
                RouteTransformType.ClientCert => Tooltips.Transform_ClientCert,
                RouteTransformType.ResponseHeadersCopy => Tooltips.Transform_ResponseHeadersCopy,
                RouteTransformType.ResponseHeader => Tooltips.Transform_ResponseHeader,
                RouteTransformType.ResponseHeaderRemove => Tooltips.Transform_ResponseHeaderRemove,
                RouteTransformType.ResponseHeadersAllowed => Tooltips.Transform_ResponseHeadersAllowed,
                RouteTransformType.ResponseTrailersCopy => Tooltips.Transform_ResponseTrailersCopy,
                RouteTransformType.ResponseTrailer => Tooltips.Transform_ResponseTrailer,
                RouteTransformType.ResponseTrailerRemove => Tooltips.Transform_ResponseTrailerRemove,
                RouteTransformType.ResponseTrailersAllowed => Tooltips.Transform_ResponseTrailersAllowed,
                _ => string.Empty
            };
        }

        public string GetName()
        {
            return type switch
            {
                RouteTransformType.RequestHeadersCopy => "Copy request headers",
                RouteTransformType.RequestHeaderOriginalHost => "Preserve Host header",
                RouteTransformType.RequestHeader => "Modify request header",
                RouteTransformType.PathRemovePrefix => "Remove path prefix",
                RouteTransformType.PathSet => "Set request path",
                RouteTransformType.PathPrefix => "Add path prefix",
                RouteTransformType.QueryRouteParameter => "Route-based query parameter",
                RouteTransformType.PathPattern => "Template path rewrite",
                RouteTransformType.QueryValueParameter => "Set query value",
                RouteTransformType.QueryRemoveParameter => "Remove query parameter",
                RouteTransformType.HttpMethodChange => "Change HTTP method",
                RouteTransformType.RequestHeaderRouteValue => "Route value header",
                RouteTransformType.RequestHeaderRemove => "Remove request header",
                RouteTransformType.RequestHeadersAllowed => "Limit request headers",
                RouteTransformType.XForwarded => "Add X-Forwarded headers",
                RouteTransformType.Forwarded => "Add Forwarded header",
                RouteTransformType.ClientCert => "Forward client certificate",
                RouteTransformType.ResponseHeadersCopy => "Copy response headers",
                RouteTransformType.ResponseHeader => "Modify response header",
                RouteTransformType.ResponseHeaderRemove => "Remove response header",
                RouteTransformType.ResponseHeadersAllowed => "Limit response headers",
                RouteTransformType.ResponseTrailersCopy => "Copy response trailers",
                RouteTransformType.ResponseTrailer => "Modify response trailer",
                RouteTransformType.ResponseTrailerRemove => "Remove response trailer",
                RouteTransformType.ResponseTrailersAllowed => "Limit response trailers",
                _ => string.Empty
            };
        }

        public RouteTransformGroup GetGroup()
        {
            return type switch
            {
                // Path / Route
                RouteTransformType.PathRemovePrefix => RouteTransformGroup.Path,
                RouteTransformType.PathSet => RouteTransformGroup.Path,
                RouteTransformType.PathPrefix => RouteTransformGroup.Path,
                RouteTransformType.PathPattern => RouteTransformGroup.Path,
                RouteTransformType.QueryRouteParameter => RouteTransformGroup.Path,

                // Query
                RouteTransformType.QueryValueParameter => RouteTransformGroup.Query,
                RouteTransformType.QueryRemoveParameter => RouteTransformGroup.Query,

                // Request Headers
                RouteTransformType.RequestHeadersCopy => RouteTransformGroup.RequestHeaders,
                RouteTransformType.RequestHeaderOriginalHost => RouteTransformGroup.RequestHeaders,
                RouteTransformType.RequestHeader => RouteTransformGroup.RequestHeaders,
                RouteTransformType.RequestHeaderRouteValue => RouteTransformGroup.RequestHeaders,
                RouteTransformType.RequestHeaderRemove => RouteTransformGroup.RequestHeaders,
                RouteTransformType.RequestHeadersAllowed => RouteTransformGroup.RequestHeaders,
                RouteTransformType.HttpMethodChange => RouteTransformGroup.RequestHeaders,
                RouteTransformType.XForwarded => RouteTransformGroup.RequestHeaders,
                RouteTransformType.Forwarded => RouteTransformGroup.RequestHeaders,
                RouteTransformType.ClientCert => RouteTransformGroup.RequestHeaders,

                // Response Headers
                RouteTransformType.ResponseHeadersCopy => RouteTransformGroup.ResponseHeaders,
                RouteTransformType.ResponseHeader => RouteTransformGroup.ResponseHeaders,
                RouteTransformType.ResponseHeaderRemove => RouteTransformGroup.ResponseHeaders,
                RouteTransformType.ResponseHeadersAllowed => RouteTransformGroup.ResponseHeaders,

                // Response Trailers
                RouteTransformType.ResponseTrailersCopy => RouteTransformGroup.ResponseTrailers,
                RouteTransformType.ResponseTrailer => RouteTransformGroup.ResponseTrailers,
                RouteTransformType.ResponseTrailerRemove => RouteTransformGroup.ResponseTrailers,
                RouteTransformType.ResponseTrailersAllowed => RouteTransformGroup.ResponseTrailers,

                _ => RouteTransformGroup.Other
            };
        }
    }
}
