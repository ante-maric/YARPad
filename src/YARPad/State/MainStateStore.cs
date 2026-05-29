namespace CodingCell.YARPad;

public record MainState(
    ThemeState Theme,
    ConfigurationProfileState ConfigurationProfile,
    CurrentConfigurationProfileState CurrentConfigurationProfile,
    YarpConfigStatusState YarpConfigStatus,
    AppInfoState AppInfo);

public class MainStateStore : CompositeStateStore<MainState>
{
    public MainStateStore(
        MainState initialState, 
        IStateStore<ThemeState> themeStore, 
        IStateStore<ConfigurationProfileState> configurationProfilesStore,
        IStateStore<CurrentConfigurationProfileState> currentConfigurationProfileStore,
        IStateStore<YarpConfigStatusState> configStore,
        IStateStore<AppInfoState> appInfoStore) 
        : base(initialState, [themeStore, configurationProfilesStore, currentConfigurationProfileStore, configStore, appInfoStore])
    {
    }
}
