using MudBlazor;

namespace CodingCell.YARPad;

internal static class ConfigurationStateExtensions
{
    extension(YARPadConfigurationStatus state)
    {
        public Severity ToSeverity()
        {
            return state switch
            {
                YARPadConfigurationStatus.Loading => Severity.Normal,
                YARPadConfigurationStatus.Invalid => Severity.Error,
                YARPadConfigurationStatus.RevertedToPrevious => Severity.Warning,
                YARPadConfigurationStatus.Applied => Severity.Success,
                _ => Severity.Normal
            };
        }
    }
}