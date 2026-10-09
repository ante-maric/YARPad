using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace CodingCell.YARPad;

public class LocalStorageStateStore<TState> : StateStore<TState>
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<LocalStorageStateStore<TState>>? _logger;

    public LocalStorageStateStore(TState initialState, ProtectedLocalStorage localStorage, ILogger<LocalStorageStateStore<TState>>? logger = null)
        : base(initialState)
    {
        _localStorage = localStorage;
        _logger = logger;
        _ = LoadAsync(initialState);
    }

    private async Task LoadAsync(TState initialState)
    {
        try
        {
            var state = await _localStorage.GetAsync<TState?>(typeof(TState).FullName!);
            Update(x => state.Value ?? initialState);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load {StateType} from local storage", typeof(TState));
        }
    }

    // Runs under the store's lock and inside the reducer guard (see StateStore.OnReduced), so it must stay
    // synchronous and must not update any store. Persisting is handed off so that a failing interop call
    // (disconnected circuit, prerendering) is logged instead of escaping as an unobserved async void exception.
    protected override void OnReduced(TState newState)
    {
        base.OnReduced(newState);

        _ = PersistAsync(newState);
    }

    private async Task PersistAsync(TState newState)
    {
        try
        {
            await _localStorage.SetAsync(typeof(TState).FullName!, newState!);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to persist {StateType} to local storage", typeof(TState));
        }
    }
}
