namespace CodingCell.YARPad;

internal interface IPolicyProvider
{
    Task<List<PolicyInfo>> GetPoliciesAsync(Guid configurationProfileID);
}
