namespace CodingCell.ReactiveStore;

/// <summary>
/// Diagnostics for state stores, meant for development.
/// </summary>
public static class StateStoreDiagnostics
{
    /// <summary>
    /// Name of the <see cref="AppContext"/> switch that turns on <see cref="DetectMutations"/>, e.g. from the app's project file:
    /// <c>&lt;RuntimeHostConfigurationOption Include="CodingCell.ReactiveStore.DetectMutations" Value="true" /&gt;</c>.
    /// </summary>
    public const string DetectMutationsSwitchName = "CodingCell.ReactiveStore.DetectMutations";

    private static volatile bool _detectMutations = AppContext.TryGetSwitch(DetectMutationsSwitchName, out var isEnabled) && isEnabled;

    /// <summary>
    /// When <see langword="true"/>, every store checks on each <see cref="StateStore{TState}.Update"/> that its state was not changed
    /// since it was published. A violation is reported after the update was applied, as an <see cref="InvalidOperationException"/>
    /// naming the changed JSON path.
    /// </summary>
    /// <remarks>
    /// Off by default: the check serializes the whole state on every update. Turn it on in development, for example
    /// <c>if (builder.Environment.IsDevelopment()) StateStoreDiagnostics.DetectMutations = true;</c>, or through the
    /// <see cref="DetectMutationsSwitchName"/> switch. It can be changed at any time; a store starts checking from its next update.
    /// </remarks>
    public static bool DetectMutations
    {
        get => _detectMutations;
        set => _detectMutations = value;
    }
}
