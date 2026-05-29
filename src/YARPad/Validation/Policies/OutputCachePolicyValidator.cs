namespace CodingCell.YARPad;

public class OutputCachePolicyValidator : PolicyValidatorBase
{
    private readonly IYarpOutputCachePolicyProvider _outputCachePolicyProvider;

    public OutputCachePolicyValidator(
        IUnifiedPolicyProvider policyProvider,
        IYarpOutputCachePolicyProvider outputCachePolicyProvider)
        : base(PolicyType.OutputCache, policyProvider, NOT_REGISTERED_IN_OPTIONS)
    {
        _outputCachePolicyProvider = outputCachePolicyProvider;
    }

    protected override async Task<bool> PolicyMustBeRegistered(PolicyInfo policy, string policyID, CancellationToken cancellationToken)
        => await _outputCachePolicyProvider.GetPolicyAsync(policyID) is not null;
}
