using FluentValidation;
using FluentValidation.Results;

namespace CodingCell.YARPad;

public class RouteValidator : PolicyValidator<RouteModel>
{
    private readonly IStoreReader<CurrentConfigurationProfileState> _currentConfigurationStateStore;

    public RouteValidator(
        IStoreReader<CurrentConfigurationProfileState> currentConfigurationStateStore,
        RouteTransformValidator routeTransformValidator, 
        RouteMetadataValidator metadataValidator,
        RouteMatchValidator matchValidator,
        IPolicyValidatorFactory policyValidatorFactory)
        : base(policyValidatorFactory)
    {
        _currentConfigurationStateStore = currentConfigurationStateStore;

        RuleFor(x => x.RouteID)
            .NotEmpty()
                .WithMessage("Route ID cannot be empty.")
            .Must(RouteIDMustBeUnique)
                .WithMessage("Route ID must be unique.");

        RuleFor(x => x.ClusterID)
            .NotEmpty()
                .WithMessage("Cluster ID is required.")
            .Must((model, clusterId) =>
            {
                var configuration = _currentConfigurationStateStore.Current.SelectedProfile?.Configuration;
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

        RuleForEach(x => x.Transforms)
            .SetValidator(routeTransformValidator);

        RuleFor(x => x.Metadata)
            .SetValidator(metadataValidator);
    }

    protected override bool PreValidate(ValidationContext<RouteModel> context, ValidationResult result)
    {
        context.RootContextData[ValidatorContext.Route.MODEL] = context.InstanceToValidate;

        return base.PreValidate(context, result);
    }

    private bool RouteIDMustBeUnique(RouteModel route, string routeID, ValidationContext<RouteModel> context)
    {
        var configuration = _currentConfigurationStateStore.Current.SelectedProfile?.Configuration;
        var originalRouteID = context.RootContextData.TryGetValue(ValidatorContext.Route.ORIGINAL_ID, out var value) ? value as string : null;

        return configuration?.Routes.TrueForAll(x => x.RouteID == originalRouteID || x.RouteID != routeID) == true;
    }
}
