namespace CodingCell.YARPad;

public sealed record ClientCertTransform() : BuiltInRouteTransform(RouteTransformType.ClientCert)
{
    public required string ClientCert { get; set; }

    public override string GetTransformSummary() =>
        $"Forward client certificate using header '{ClientCert}'";
}
