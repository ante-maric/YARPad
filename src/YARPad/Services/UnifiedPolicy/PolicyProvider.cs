namespace CodingCell.YARPad;

internal abstract class PolicyProvider(
    PolicyType policyType,
    IStoreReader<ConfigurationProfileState> configurationProfileStore,
    Type? policyConstantsType,
    Func<string, string?>? descriptionFunc = null) : IPolicyProvider
{
    private readonly PolicyType _policyType = policyType;
    private readonly IStoreReader<ConfigurationProfileState> _configurationProfileStore = configurationProfileStore;
    private readonly Type? _policyConstantsType = policyConstantsType;
    private readonly Func<string, string?> _descriptionFunc = descriptionFunc ?? (x => null);

    public async Task<List<PolicyInfo>> GetPoliciesAsync(Guid configurationProfileID)
    {
        var configuration = _configurationProfileStore.Current.Profiles.Find(x => x.ID == configurationProfileID)?.Configuration;
        if (configuration == null)
            return [];

        return (_policyConstantsType != null ? ConfigOptionExtractor.GetOptions(_policyConstantsType) : [])
            .ConvertAll(x => new PolicyInfo() { ID = x.ID, Name = x.ID.HumanizeTitle(), IsBuiltIn = true, Description = _descriptionFunc(x.ID) })
            .Concat(configuration.Policies[_policyType].Select(x => x with { }))
            .OrderBy(x => x.IsBuiltIn ? 1 : 0)
            .ThenBy(x => x.ID)
            .ToList();
    }
}
