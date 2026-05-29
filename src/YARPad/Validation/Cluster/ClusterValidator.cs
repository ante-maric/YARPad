using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.YARPad;

public class ClusterValidator : PolicyValidator<ClusterModel>
{
    private readonly IStoreReader<CurrentConfigurationProfileState> _currentConfigurationStateStore;

    public ClusterValidator(
        IStoreReader<CurrentConfigurationProfileState> currentConfigurationStateStore,
        IPolicyValidatorFactory policyValidatorFactory,
        DestinationValidator destinationValidator,
        SessionAffinityValidator sessionValidator,
        HealthCheckValidator healthValidator,
        ForwarderRequestValidator httpRequestValidator,
        HttpClientValidator httpClientValidator,
        ClusterMetadataValidator metadataValidator)
        : base(policyValidatorFactory)
    {
        _currentConfigurationStateStore = currentConfigurationStateStore;

        RuleFor(x => x.ClusterID)
            .NotEmpty()
                .WithMessage("Cluster ID cannot be empty.")
            .Must(ClusterIDMustBeUnique)
                .WithMessage("Cluster ID must be unique.");

        RuleForEach(x => x.Destinations)
            .SetValidator(destinationValidator);

        RuleFor(x => x.Destinations)
            .Must(x => HaveUniqueIDs(x, x => x.ID))
                .WithMessage("Destination IDs must be unique (case-sensitive).");

        RuleFor(x => x.SessionAffinity)
            .SetValidator(sessionValidator);

        RuleFor(x => x.HealthCheck)
            .SetValidator(healthValidator);

        RuleFor(x => x.HttpRequest)
            .SetValidator(httpRequestValidator);

        RuleFor(x => x.HttpClient)
            .SetValidator(httpClientValidator);

        RuleFor(x => x.Metadata)
            .SetValidator(metadataValidator)
            .Must(x => HaveUniqueIDs(x, x => x.Key))
                .WithMessage("Metadata must have unique keys (case-sensitive).");

        RuleFor(x => x.LoadBalancingPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.LoadBalancing, token));
    }

    protected override bool PreValidate(ValidationContext<ClusterModel> context, ValidationResult result)
    {
        context.RootContextData[ValidatorContext.Cluster.MODEL] = context.InstanceToValidate;

        return base.PreValidate(context, result);
    }

    private bool ClusterIDMustBeUnique(ClusterModel model, string clusterID, ValidationContext<ClusterModel> context)
    {
        var configuration = _currentConfigurationStateStore.Current.SelectedProfile?.Configuration;
        var originalClusterID = context.RootContextData.TryGetValue(ValidatorContext.Cluster.ORIGINAL_ID, out var value) ? value as string : null;

        return configuration?.Clusters.TrueForAll(x => x.ClusterID == originalClusterID || x.ClusterID != clusterID) == true;
    }
}
