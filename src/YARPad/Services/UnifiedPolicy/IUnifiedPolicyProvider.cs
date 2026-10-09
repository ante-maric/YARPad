namespace CodingCell.YARPad;

public interface IUnifiedPolicyProvider
{
    Task<List<PolicyInfo>> GetPoliciesAsync(Guid configurationProfileID, PolicyType policyType, CancellationToken cancellationToken);
}
