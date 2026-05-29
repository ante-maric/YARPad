namespace CodingCell.YARPad;

internal static class ValidatorContext
{
    internal static class Policy
    {
        public const string ORIGINAL_ID = "OriginalPolicyID";
        public const string IS_EDITING = "IsEditingPolicy";
    }

    internal static class Cluster
    {
        public const string ORIGINAL_ID = "OriginalClusterID";
        public const string MODEL = nameof(ClusterModel);
        public const string IS_EDITING_DESTINATION = "IsEditingDestination";
    }

    internal static class Route
    {
        public const string ORIGINAL_ID = "OriginalRouteID";
        public const string MODEL = nameof(RouteModel);
    }

    internal static class Destination
    {
        public const string ORIGINAL_ID = "OriginalDestinationID";
    }

    internal static class CustomTransform
    {
        public const string ORIGINAL_TYPE = "OriginalTransformType";
        public const string ORIGINAL_PARAMETER_NAME = "OriginalParameterName";
        public const string MODEL = nameof(CustomTransformDefinition);
        public const string IS_EDITING_PARAMETER = "IsEditingParameter";
        public const string IS_EDITING = "IsEditingCustomTransform";
    }

    internal static class Metadata
    {
        public const string ORIGINAL_KEY = "OriginalMetadataKey";
        public const string LIST = "Metadata";
        public const string IS_EDITING = "IsEditingMetadata";
    }
}