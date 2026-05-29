using CodingCell.YARPad.Data;

namespace CodingCell.YARPad;

internal static class YARPadConfigurationEntityListExtensions
{
    extension(List<YARPadConfigurationEntity> list)
    {
        public ConfigurationProfileState ToState()
        {
            var profiles = list.ConvertAll(x => x.ToConfigurationProfile());
            var activeProfile = profiles.FirstOrDefault(x => x.IsActive);

            return new(profiles, activeProfile);
        }
    }
}