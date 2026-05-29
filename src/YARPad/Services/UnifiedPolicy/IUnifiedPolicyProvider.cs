namespace CodingCell.YARPad;

public interface IUnifiedPolicyProvider
{
    Task<List<PolicyInfo>> GetPoliciesAsync(PolicyType policyType, CancellationToken cancellationToken);
}
