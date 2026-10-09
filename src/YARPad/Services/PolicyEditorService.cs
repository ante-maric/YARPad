using MudBlazor;
using Microsoft.Extensions.Logging;
using CodingCell.YARPad.Components.Policy;

namespace CodingCell.YARPad;

internal class PolicyEditorService(
    IDialogService dialogService,
    IYARPadConfigurationProvider configurationProvider,
    IUnifiedPolicyProvider policyProvider,
    IStateStore<ConfigurationProfileState> stateStore,
    ILogger<PolicyEditorService> logger) : IPolicyEditorService
{
    public Task<bool> CreateAsync(Guid configurationProfileID, PolicyType policyType)
    {
        return OpenEditorAsync(configurationProfileID, policyType, new PolicyInfo() { ID = "", Name = "" });
    }

    public async Task<bool> OpenAsync(Guid configurationProfileID, PolicyType policyType, string policyID)
    {
        // Built-in policies are not stored in the configuration and cannot be edited.
        var policy = FindCustomPolicy(configurationProfileID, policyType, policyID);
        if (policy == null)
            return false;

        return await OpenEditorAsync(configurationProfileID, policyType, policy with { }, policyID);
    }

    public async Task<bool> CloneAsync(Guid configurationProfileID, PolicyType policyType, string policyID)
    {
        var policy = FindCustomPolicy(configurationProfileID, policyType, policyID)
            ?? (await policyProvider.GetPoliciesAsync(configurationProfileID, policyType, default)).Find(x => x.IsBuiltIn && x.ID == policyID);
        if (policy == null)
            return false;

        // A clone is always a custom policy, also when cloning a built-in one.
        return await OpenEditorAsync(configurationProfileID, policyType, policy with { ID = policy.ID + " Cloned", IsBuiltIn = false });
    }

    private PolicyInfo? FindCustomPolicy(Guid configurationProfileID, PolicyType policyType, string policyID)
    {
        var policies = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID)?.Configuration.Policies;
        return policies != null && policies.TryGetValue(policyType, out var typePolicies) ? typePolicies.Find(x => x.ID == policyID) : null;
    }

    // policy is a draft the editor may change; never an object from the store.
    private async Task<bool> OpenEditorAsync(Guid configurationProfileID, PolicyType policyType, PolicyInfo policy, string? policyID = null)
    {
        var profile = stateStore.Current.Profiles.FirstOrDefault(x => x.ID == configurationProfileID);
        if (profile == null)
        {
            logger.LogWarning("Configuration profile {ConfigurationProfileID} not found for policy editor", configurationProfileID);
            return false;
        }

        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
        };

        var parameters = new DialogParameters<PolicyDialog>()
        {
            { x => x.PolicyID, policyID },
            { x => x.PolicyType, policyType },
            { x => x.Policy, policy },
            { x => x.ConfigurationProfileID, profile.ID },
        };

        var dialog = await dialogService.ShowAsync<PolicyDialog>(null, parameters, options);
        var result = await dialog.Result;

        if (result == null || result.Canceled || result.Data is not PolicyInfo savedPolicy)
        {
            logger.LogDebug("Policy dialog was canceled or returned no data");
            return false;
        }

        try
        {
            await configurationProvider.SavePolicyAsync(profile.ID, policyID, savedPolicy, policyType);
            logger.LogInformation("Saved {PolicyType} policy {PolicyID} to configuration profile {ConfigurationProfileID}", policyType, savedPolicy.ID, profile.ID);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save {PolicyType} policy {PolicyID} to configuration profile {ConfigurationProfileID}", policyType, policyID ?? "<new>", profile.ID);
            throw;
        }
    }
}
