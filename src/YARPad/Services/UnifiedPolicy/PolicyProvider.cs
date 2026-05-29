namespace CodingCell.YARPad;

internal abstract class PolicyProvider(
    PolicyType policyType,
    IStoreReader<CurrentConfigurationProfileState> currentConfigurationStateStore,
    Type? policyConstantsType,
    Func<string, string?>? descriptionFunc = null) : IPolicyProvider
{
    private readonly PolicyType _policyType = policyType;
    private readonly IStoreReader<CurrentConfigurationProfileState> _currentConfigurationStateStore = currentConfigurationStateStore;
    private readonly Type? _policyConstantsType = policyConstantsType;
    private readonly Func<string, string?> _descriptionFunc = descriptionFunc ?? (x => null);

    public async Task<List<PolicyInfo>> GetPoliciesAsync()
    {
        var configuration = _currentConfigurationStateStore.Current.SelectedProfile?.Configuration;
        if (configuration == null)
            return [];

        return (_policyConstantsType != null ? ConfigOptionExtractor.GetOptions(_policyConstantsType) : [])
            .ConvertAll(x => new PolicyInfo() { ID = x.ID, Name = x.ID.HumanizeTitle(), IsBuiltIn = true, Description = _descriptionFunc(x.ID) })
            .Concat(configuration.Policies[_policyType])
            .OrderBy(x => x.IsBuiltIn ? 1 : 0)
            .ThenBy(x => x.ID)
            .ToList();
    }
}
