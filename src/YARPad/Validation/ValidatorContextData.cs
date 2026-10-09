namespace CodingCell.YARPad;

internal static class ValidatorContextData
{
    public static Dictionary<string, object?> CreatePolicy(string? originalPolicyID, Guid configurationProfileID)
        => new()
        {
            [ValidatorContext.Profile.ID] = configurationProfileID,
            [ValidatorContext.Policy.IS_EDITING] = true,
            [ValidatorContext.Policy.ORIGINAL_ID] = originalPolicyID,
        };

    public static Dictionary<string, object?> CreateCluster(string? originalClusterID, Guid configurationProfileID)
        => new()
        {
            [ValidatorContext.Profile.ID] = configurationProfileID,
            [ValidatorContext.Cluster.ORIGINAL_ID] = originalClusterID,
        };

    public static Dictionary<string, object?> CreateDestination(
        string? originalDestinationID,
        ClusterModel cluster)
        => new()
        {
            [ValidatorContext.Cluster.IS_EDITING_DESTINATION] = true,
            [ValidatorContext.Destination.ORIGINAL_ID] = originalDestinationID,
            [ValidatorContext.Cluster.MODEL] = cluster,
        };

    public static Dictionary<string, object?> CreateRoute(string? originalRouteID, Guid configurationProfileID)
        => new()
        {
            [ValidatorContext.Profile.ID] = configurationProfileID,
            [ValidatorContext.Route.ORIGINAL_ID] = originalRouteID,
        };

    public static Dictionary<string, object?> CreateRouteTransform(RouteModel route, bool isCustomTransform)
    {
        var contextData = new Dictionary<string, object?>
        {
            [ValidatorContext.Route.MODEL] = route,
        };

        if (isCustomTransform)
            contextData[ValidatorContext.CustomTransform.IS_EDITING] = true;

        return contextData;
    }

    public static Dictionary<string, object?> CreateCustomTransform(string? originalType, Guid configurationProfileID)
        => new()
        {
            [ValidatorContext.Profile.ID] = configurationProfileID,
            [ValidatorContext.CustomTransform.ORIGINAL_TYPE] = originalType,
        };

    public static Dictionary<string, object?> CreateCustomTransformParameter(
        string? originalParameterName,
        CustomTransformDefinition model)
        => new()
        {
            [ValidatorContext.CustomTransform.IS_EDITING_PARAMETER] = true,
            [ValidatorContext.CustomTransform.ORIGINAL_PARAMETER_NAME] = originalParameterName,
            [ValidatorContext.CustomTransform.MODEL] = model,
        };

    public static Dictionary<string, object?> CreateMetadata(
        string? originalKey,
        List<YarpMetadata> metadataList)
        => new()
        {
            [ValidatorContext.Metadata.IS_EDITING] = true,
            [ValidatorContext.Metadata.ORIGINAL_KEY] = originalKey,
            [ValidatorContext.Metadata.LIST] = metadataList,
        };

    public static Guid? GetConfigurationProfileID(this FluentValidation.IValidationContext context)
        => context.RootContextData.TryGetValue(ValidatorContext.Profile.ID, out var value) ? value as Guid? : null;

    /// <summary>
    /// The configuration of the profile being edited, as set by <see cref="ValidatorContext.Profile.ID"/>.
    /// </summary>
    public static YARPadConfiguration? GetConfiguration(this FluentValidation.IValidationContext context, IStoreReader<ConfigurationProfileState> configurationProfileStore)
        => context.GetConfigurationProfileID() is Guid id ? configurationProfileStore.Current.Profiles.Find(x => x.ID == id)?.Configuration : null;

    public static void SetClusterModel(this IDictionary<string, object?> contextData, ClusterModel cluster)
        => contextData[ValidatorContext.Cluster.MODEL] = cluster;

    public static ClusterModel? GetClusterModel(this FluentValidation.IValidationContext context)
        => context.RootContextData.TryGetValue(ValidatorContext.Cluster.MODEL, out var value) ? value as ClusterModel : null;

    public static void SetOriginalClusterID(this IDictionary<string, object?> contextData, string? originalClusterID)
        => contextData[ValidatorContext.Cluster.ORIGINAL_ID] = originalClusterID;

    public static string? GetOriginalClusterID(this FluentValidation.IValidationContext context)
        => context.RootContextData.TryGetValue(ValidatorContext.Cluster.ORIGINAL_ID, out var value) ? value as string : null;

    public static void SetIsEditingDestination(this IDictionary<string, object?> contextData, bool isEditing)
        => contextData[ValidatorContext.Cluster.IS_EDITING_DESTINATION] = isEditing;

    public static bool GetIsEditingDestination(this FluentValidation.IValidationContext context)
        => context.RootContextData.TryGetValue(ValidatorContext.Cluster.IS_EDITING_DESTINATION, out var value) && value is true;

    public static void SetOriginalDestinationID(this IDictionary<string, object?> contextData, string? originalDestinationID)
        => contextData[ValidatorContext.Destination.ORIGINAL_ID] = originalDestinationID;

    public static string? GetOriginalDestinationID(this FluentValidation.IValidationContext context)
        => context.RootContextData.TryGetValue(ValidatorContext.Destination.ORIGINAL_ID, out var value) ? value as string : null;
}
