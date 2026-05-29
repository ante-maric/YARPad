using Microsoft.AspNetCore.Components;

namespace CodingCell.YARPad.Components.Common;

public class StateLayoutComponentBase<TState> : LayoutComponentBase, IDisposable
{
    private IDisposable? _subscription;

    [Inject]
    private IStoreReader<TState> StateStore { get; set; } = default!;

    [Inject]
    public TState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Subscribe();
    }

    protected virtual void Subscribe()
    {
        _subscription = StateStore.Changes.Subscribe(RefreshState);
    }

    protected virtual void RefreshState(TState state)
    {
        State = state;
        _ = InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        _subscription?.Dispose();
    }
}
