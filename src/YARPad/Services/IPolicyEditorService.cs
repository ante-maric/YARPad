namespace CodingCell.YARPad;

internal interface IPolicyEditorService
{
    Task<bool> CreateAsync(Guid configurationProfileID, PolicyType policyType);
    Task<bool> OpenAsync(Guid configurationProfileID, PolicyType policyType, string policyID);
    Task<bool> CloneAsync(Guid configurationProfileID, PolicyType policyType, string policyID);
}
