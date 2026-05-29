using System.Reflection;

namespace CodingCell.YARPad;

/// <summary>
/// Holds additional assemblies to include in Blazor component and route discovery.
/// Register assemblies during service configuration; the Router and endpoint mapper consume them at runtime.
/// </summary>
public sealed class RazorAssemblyRegistry
{
    private readonly List<Assembly> _assemblies = [];

    public IReadOnlyList<Assembly> Assemblies => _assemblies;

    public void Add(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
            _assemblies.Add(assembly);
    }
}
