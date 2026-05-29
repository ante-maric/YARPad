using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CodingCell.YARPad;

public class LocalStorageStateStore<TState> : StateStore<TState>
{
    private readonly ProtectedLocalStorage _localStorage;

    public LocalStorageStateStore(TState initialState, ProtectedLocalStorage localStorage) 
        : base(initialState)
    {
        _localStorage = localStorage;
        _ = LoadAsync(initialState);
    }

    private async Task LoadAsync(TState initialState)
    {
        var state = await _localStorage.GetAsync<TState?>(typeof(TState).FullName!);
        Update(x => state.Value ?? initialState);
    }

    protected override async void OnReduced(TState newState)
    {
        base.OnReduced(newState);

        await _localStorage.SetAsync(typeof(TState).FullName!, newState!);
    }
}
