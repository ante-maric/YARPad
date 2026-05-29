namespace CodingCell.YARPad;

internal static class PolicyContextExtensions
{
    extension(PolicyContext policyContext)
    {
        public IEnumerable<PolicyType> GetPolicyTypes() => Enum.GetValues<PolicyType>().Where(x => x.GetContext() == policyContext);
    }
}