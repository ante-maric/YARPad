using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.YARPad;

public class RouteValidator : PolicyValidator<RouteModel>
{
    private readonly IStoreReader<ConfigurationProfileState> _configurationProfileStore;

    public RouteValidator(
        IStoreReader<ConfigurationProfileState> configurationProfileStore,
        RouteTransformValidator routeTransformValidator, 
        RouteMetadataValidator metadataValidator,
        RouteMatchValidator matchValidator,
        IPolicyValidatorFactory policyValidatorFactory)
        : base(policyValidatorFactory)
    {
        _configurationProfileStore = configurationProfileStore;

        RuleFor(x => x.RouteID)
            .NotEmpty()
                .WithMessage("Route ID cannot be empty.")
            .Must(RouteIDMustBeUnique)
                .WithMessage("Route ID must be unique.");

        RuleFor(x => x.ClusterID)
            .NotEmpty()
                .WithMessage("Cluster ID is required.")
            .Must((model, clusterId, context) =>
            {
                var configuration = context.GetConfiguration(_configurationProfileStore);
                return configuration?.Clusters.Any(x => x.ClusterID == clusterId) == true;
            })
                .WithMessage("Cluster ID must refer to an existing cluster.");

        RuleFor(x => x.AuthorizationPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.Authorization, token));

        RuleFor(x => x.RateLimiterPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.RateLimiter, token));

        RuleFor(x => x.OutputCachePolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.OutputCache, token));

        RuleFor(x => x.CorsPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.Cors, token));

        RuleFor(x => x.TimeoutPolicy)
            .CustomAsync((policyID, ctx, token) => ValidatePolicyAsync(policyID, ctx, PolicyType.Timeout, token));

        RuleFor(x => x.Match)
            .SetValidator(matchValidator);

        // Disabled sections are not sent to YARP (see AutoMapperProfile), so they are not validated either.
        RuleForEach(x => x.Transforms)
            .SetValidator(routeTransformValidator)
                .When(x => x.IsSectionEnabled(RouteConfigSection.Transform));

        RuleFor(x => x.Metadata)
            .SetValidator(metadataValidator)
                .When(x => x.IsSectionEnabled(RouteConfigSection.Metadata));
    }

    protected override bool PreValidate(ValidationContext<RouteModel> context, ValidationResult result)
    {
        context.RootContextData[ValidatorContext.Route.MODEL] = context.InstanceToValidate;

        return base.PreValidate(context, result);
    }

    private bool RouteIDMustBeUnique(RouteModel route, string routeID, ValidationContext<RouteModel> context)
    {
        var configuration = context.GetConfiguration(_configurationProfileStore);
        var originalRouteID = context.RootContextData.TryGetValue(ValidatorContext.Route.ORIGINAL_ID, out var value) ? value as string : null;

        return configuration?.Routes.TrueForAll(x => x.RouteID == originalRouteID || x.RouteID != routeID) == true;
    }
}
