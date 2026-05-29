using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace CodingCell.YARPad;

// TODO: update or remove this once AspNetCore provides a mechanism to validate the OutputCache policies https://github.com/dotnet/aspnetcore/issues/52419
public interface IYarpOutputCachePolicyProvider
{
    ValueTask<object?> GetPolicyAsync(string policyName);
}

internal sealed class YarpOutputCachePolicyProvider : IYarpOutputCachePolicyProvider
{
    // Workaround for https://github.com/dotnet/yarp/issues/2598 to make YARP work with NativeAOT on .NET 8. This is not needed on .NET 9+.
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static readonly Type s_OutputCacheOptionsType = typeof(OutputCacheOptions);

    private readonly OutputCacheOptions _outputCacheOptions;

    private readonly IDictionary _policyMap;

    public YarpOutputCachePolicyProvider(IOptions<OutputCacheOptions> outputCacheOptions)
    {
        ArgumentNullException.ThrowIfNull(outputCacheOptions?.Value);
        _outputCacheOptions = outputCacheOptions.Value;

        var property = s_OutputCacheOptionsType.GetProperty("NamedPolicies", BindingFlags.Instance | BindingFlags.NonPublic);
        if (property == null || !typeof(IDictionary).IsAssignableFrom(property.PropertyType))
        {
            throw new NotSupportedException("This version of YARP is incompatible with the current version of ASP.NET Core.");
        }
        _policyMap = (property.GetValue(_outputCacheOptions, null) as IDictionary) ?? new Dictionary<string, object>();
    }

    public ValueTask<object?> GetPolicyAsync(string policyName)
    {
        return ValueTask.FromResult(_policyMap[policyName]);
    }
}
