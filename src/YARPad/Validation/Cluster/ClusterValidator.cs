using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.YARPad;

public class ClusterValidator : PolicyValidator<ClusterModel>
{
    private readonly IStoreReader<ConfigurationProfileState> _configurationProfileStore;

    public ClusterValidator(
        IStoreReader<ConfigurationProfileState> configurationProfileStore,
        IPolicyValidatorFactory policyValidatorFactory,
        DestinationValidator destinationValidator,
        SessionAffinityValidator sessionValidator,
        HealthCheckValidator healthValidator,
        ForwarderRequestValidator httpRequestValidator,
        HttpClientValidator httpClientValidator,
        ClusterMetadataValidator metadataValidator)
        : base(policyValidatorFactory)
    {
        _configurationProfileStore = configurationProfileStore;

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

        // Disabled sections are not sent to YARP (see AutoMapperProfile), so they are not validated either.
        RuleFor(x => x.SessionAffinity)
            .SetValidator(sessionValidator)
                .When(x => x.IsSectionEnabled(ClusterConfigSection.SessionAffinity));

        RuleFor(x => x.HealthCheck)
            .SetValidator(healthValidator)
                .When(x => x.IsSectionEnabled(ClusterConfigSection.HealthCheck));

        RuleFor(x => x.HttpRequest)
            .SetValidator(httpRequestValidator)
                .When(x => x.IsSectionEnabled(ClusterConfigSection.HttpRequest));

        RuleFor(x => x.HttpClient)
            .SetValidator(httpClientValidator)
                .When(x => x.IsSectionEnabled(ClusterConfigSection.HttpClient));

        RuleFor(x => x.Metadata)
            .SetValidator(metadataValidator)
                .When(x => x.IsSectionEnabled(ClusterConfigSection.Metadata));

        RuleFor(x => x.Metadata)
            .Must(x => HaveUniqueIDs(x, x => x.Key))
                .WithMessage("Metadata must have unique keys (case-sensitive).")
                .When(x => x.IsSectionEnabled(ClusterConfigSection.Metadata));

        RuleFor(x => x.LoadBalancingPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.LoadBalancing, token));
    }

    protected override bool PreValidate(ValidationContext<ClusterModel> context, ValidationResult result)
    {
        context.RootContextData.SetClusterModel(context.InstanceToValidate);

        return base.PreValidate(context, result);
    }

    private bool ClusterIDMustBeUnique(ClusterModel model, string clusterID, ValidationContext<ClusterModel> context)
    {
        var configuration = context.GetConfiguration(_configurationProfileStore);
        var originalClusterID = context.GetOriginalClusterID();

        return configuration?.Clusters.TrueForAll(x => x.ClusterID == originalClusterID || x.ClusterID != clusterID) == true;
    }
}
