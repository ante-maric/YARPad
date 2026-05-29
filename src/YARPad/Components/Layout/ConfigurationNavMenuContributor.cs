namespace CodingCell.YARPad.Components.Layout;

internal sealed class ConfigurationNavMenuContributor : INavMenuContributor
{
    public int Order => 0;

    public Type NavSectionComponentType => typeof(ConfigurationNavSection);
}
