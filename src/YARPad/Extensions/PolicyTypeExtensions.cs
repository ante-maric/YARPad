namespace CodingCell.YARPad;

internal static class PolicyTypeExtensions
{
    extension(PolicyType policyType)
    {
        public PolicyContext GetContext()
        {
            if (policyType >= PolicyType.LoadBalancing)
                return PolicyContext.Cluster;

            return PolicyContext.Route;
        }
    }
}

internal static class PolicyTypeCollectionExtensions
{
    extension(Dictionary<PolicyType, List<PolicyInfo>> dict)
    {
        public string? GetPolicyDescription(PolicyType policyType, string? policyID)
        {
            if (policyID == null)
                return null;

            return dict.GetValueOrDefault(policyType, []).GetPolicyDescription(policyID);
        }
    }

    extension(List<PolicyInfo> list)
    {
        public string? GetPolicyDescription(string? policyID)
        {
            if (policyID == null)
                return null;

            return list.Find(x => x.ID == policyID)?.Description;
        }
    }
}
