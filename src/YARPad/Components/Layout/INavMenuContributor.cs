namespace CodingCell.YARPad.Components.Layout;

/// <summary>
/// Contributes a navigation section to the application's side menu.
/// Implement this interface and register it with DI to add custom navigation.
/// </summary>
public interface INavMenuContributor
{
    /// <summary>
    /// Controls render order. Lower values appear first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// The Blazor component type that renders this navigation section.
    /// The component receives no parameters; use DI for any dependencies.
    /// </summary>
    Type NavSectionComponentType { get; }
}
