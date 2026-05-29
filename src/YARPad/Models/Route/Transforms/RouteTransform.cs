using System.Text.Json.Serialization;

namespace CodingCell.YARPad;

public abstract record BuiltInRouteTransform : RouteTransform
{
    private readonly RouteTransformType _routeTransformType;

    public override string TransformType => _routeTransformType.ToString();
    public override string Name => _routeTransformType.GetName();
    public override string Description
    {
        get => _routeTransformType.GetDescription();
        set { }
    }

    protected BuiltInRouteTransform(RouteTransformType routeTransformType)
    {
        _routeTransformType = routeTransformType;
    }

    public override RouteTransformGroup GetGroup() => _routeTransformType.GetGroup();
}

[JsonDerivedType(typeof(RequestHeadersCopyTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeadersCopy))]
[JsonDerivedType(typeof(RequestHeaderOriginalHostTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeaderOriginalHost))]
[JsonDerivedType(typeof(RequestHeaderTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeader))]
[JsonDerivedType(typeof(PathRemovePrefixTransform), typeDiscriminator: nameof(RouteTransformType.PathRemovePrefix))]
[JsonDerivedType(typeof(PathSetTransform), typeDiscriminator: nameof(RouteTransformType.PathSet))]
[JsonDerivedType(typeof(PathPrefixTransform), typeDiscriminator: nameof(RouteTransformType.PathPrefix))]
[JsonDerivedType(typeof(QueryRouteParameterTransform), typeDiscriminator: nameof(RouteTransformType.QueryRouteParameter))]
[JsonDerivedType(typeof(PathPatternTransform), typeDiscriminator: nameof(RouteTransformType.PathPattern))]
[JsonDerivedType(typeof(QueryValueParameterTransform), typeDiscriminator: nameof(RouteTransformType.QueryValueParameter))]
[JsonDerivedType(typeof(QueryRemoveParameterTransform), typeDiscriminator: nameof(RouteTransformType.QueryRemoveParameter))]
[JsonDerivedType(typeof(HttpMethodChangeTransform), typeDiscriminator: nameof(RouteTransformType.HttpMethodChange))]
[JsonDerivedType(typeof(RequestHeaderRouteValueTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeaderRouteValue))]
[JsonDerivedType(typeof(RequestHeaderRemoveTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeaderRemove))]
[JsonDerivedType(typeof(RequestHeadersAllowedTransform), typeDiscriminator: nameof(RouteTransformType.RequestHeadersAllowed))]
[JsonDerivedType(typeof(XForwardedTransform), typeDiscriminator: nameof(RouteTransformType.XForwarded))]
[JsonDerivedType(typeof(ForwardedTransform), typeDiscriminator: nameof(RouteTransformType.Forwarded))]
[JsonDerivedType(typeof(ClientCertTransform), typeDiscriminator: nameof(RouteTransformType.ClientCert))]
[JsonDerivedType(typeof(ResponseHeadersCopyTransform), typeDiscriminator: nameof(RouteTransformType.ResponseHeadersCopy))]
[JsonDerivedType(typeof(ResponseHeaderTransform), typeDiscriminator: nameof(RouteTransformType.ResponseHeader))]
[JsonDerivedType(typeof(ResponseHeaderRemoveTransform), typeDiscriminator: nameof(RouteTransformType.ResponseHeaderRemove))]
[JsonDerivedType(typeof(ResponseHeadersAllowedTransform), typeDiscriminator: nameof(RouteTransformType.ResponseHeadersAllowed))]
[JsonDerivedType(typeof(ResponseTrailersCopyTransform), typeDiscriminator: nameof(RouteTransformType.ResponseTrailersCopy))]
[JsonDerivedType(typeof(ResponseTrailerTransform), typeDiscriminator: nameof(RouteTransformType.ResponseTrailer))]
[JsonDerivedType(typeof(ResponseTrailerRemoveTransform), typeDiscriminator: nameof(RouteTransformType.ResponseTrailerRemove))]
[JsonDerivedType(typeof(ResponseTrailersAllowedTransform), typeDiscriminator: nameof(RouteTransformType.ResponseTrailersAllowed))]
[JsonDerivedType(typeof(CustomTransform), typeDiscriminator: "Custom")]
public abstract record RouteTransform
{
    public abstract string TransformType { get; }
    public abstract string Name { get; }
    public abstract string Description { get; set; }

    public abstract RouteTransformGroup GetGroup();
    public abstract string GetTransformSummary();

    protected static string DescribeSetAppend(string name, string? set, string? append, string label, ResponseTransformCondition? when = null)
    {
        if (!string.IsNullOrWhiteSpace(set))
            return $"Set {label} '{name}' to '{set}'{DescribeCondition(when)}";

        if (!string.IsNullOrWhiteSpace(append))
            return $"Append {label} '{name}' with '{append}'{DescribeCondition(when)}";

        return $"Update {label} '{name}'{DescribeCondition(when)}";
    }

    protected static string DescribeRemove(string name, string label, ResponseTransformCondition? when = null) =>
        $"Remove {label} '{name}'{DescribeCondition(when)}";

    protected static string DescribeList(string label, IReadOnlyCollection<string> items) =>
        items.Count == 0 ? $"{label}: none" : $"{label}: {string.Join(", ", items)}";

    private static string DescribeCondition(ResponseTransformCondition? when)
    {
        return when switch
        {
            ResponseTransformCondition.Always => " unconditionally",
            ResponseTransformCondition.Success => " when status is success",
            ResponseTransformCondition.Failure => " when status is failure",
            _ => string.Empty
        };
    }
}
