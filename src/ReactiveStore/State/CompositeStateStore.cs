using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace CodingCell.ReactiveStore;

/// <summary>
/// State store whose state is composed from child stores and recomposed whenever one of them changes.
/// </summary>
/// <remarks>
/// The composer runs as a reducer (see <see cref="StateStore{TState}"/>): it must only read the child stores'
/// <c>Current</c> and must not update any store. Because it reads the children's latest state under this
/// store's lock, the last recomposition always reflects the newest child states, even under concurrent updates.
/// </remarks>
public class CompositeStateStore<TState> : StateStore<TState>
{
    private readonly Func<TState> _composer;
    private readonly CompositeDisposable _subscriptions;
    private readonly ILogger<CompositeStateStore<TState>>? _logger;
    private readonly Lock _batchGate = new();
    private int _batchDepth;
    private bool _hasPendingChanges;

    public CompositeStateStore(
        TState initialState,
        Func<TState> composer,
        IReadOnlyList<IStoreReader<object>> stateStores,
        ILogger<CompositeStateStore<TState>>? logger = null)
        : base(initialState)
    {
        ArgumentNullException.ThrowIfNull(composer);
        ArgumentNullException.ThrowIfNull(stateStores);

        _composer = composer;
        _logger = logger;
        _subscriptions = [];

        foreach (var store in stateStores)
            _subscriptions.Add(store.Changes.Subscribe(_ => OnChildStateChanged()));
    }

    /// <summary>
    /// Runs <paramref name="updates"/> and recomposes once at the end, so multiple child store
    /// updates produce a single composed state emission. Batches can be nested.
    /// </summary>
    public void Batch(Action updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        lock (_batchGate)
            _batchDepth++;

        try
        {
            updates();
        }
        finally
        {
            bool shouldRecompose;
            lock (_batchGate)
            {
                shouldRecompose = --_batchDepth == 0 && _hasPendingChanges;
                if (shouldRecompose)
                    _hasPendingChanges = false;
            }

            // Recompose even when updates threw: child stores that were already updated still changed.
            if (shouldRecompose)
                Recompose();
        }
    }

    private void OnChildStateChanged()
    {
        lock (_batchGate)
        {
            if (_batchDepth > 0)
            {
                _hasPendingChanges = true;
                return;
            }
        }

        Recompose();
    }

    private void Recompose()
    {
        try
        {
            Update(_ => _composer());
        }
        catch (Exception ex)
        {
            // Fallback: composition failed, keep the current state but surface the failure so it can be diagnosed.
            _logger?.LogError(ex, "Failed to compose state for {StateType}", typeof(TState));
        }
    }

    public override void Dispose()
    {
        _subscriptions.Dispose();
        base.Dispose();
    }
}
