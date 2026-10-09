using System.Reactive.Subjects;

namespace CodingCell.ReactiveStore;

/// <summary>
/// Thread-safe state store.
/// </summary>
/// <remarks>
/// Deadlock freedom: the store's lock is held only while the reducer (and <see cref="OnReduced"/>) runs,
/// never while subscribers are notified, and a store may not be updated from inside a reducer, so a
/// thread never holds two store locks at once. Reducers must therefore be pure: compute the next state
/// from the current one and read other stores' <see cref="Current"/> if needed, but never call
/// <see cref="Update"/>.
/// <para>
/// Notifications are delivered in update order, one at a time. If another thread is already delivering,
/// <see cref="Update"/> returns after queueing its state and that thread delivers it, so <see cref="Current"/>
/// can be ahead of the last value subscribers have seen. An update made by a subscriber while it is being
/// notified is delivered after the current notification completes, not recursively.
/// </para>
/// <para>
/// A published state must not be changed; only <see cref="Update"/> replaces it. With <see cref="StateStoreDiagnostics.DetectMutations"/>
/// on, the store checks this: when its state was changed since it was published, the next <see cref="Update"/> is still applied
/// and then throws an <see cref="InvalidOperationException"/> naming the changed path.
/// </para>
/// </remarks>
public class StateStore<TState> : IStateStore<TState>
{
    // Boxed so Current can be read without a lock for any TState, including value types.
    private sealed class Snapshot(TState value)
    {
        public TState Value { get; } = value;
    }

    private readonly Lock _gate = new();
    private readonly BehaviorSubject<TState> _subject;
    private readonly Queue<TState> _pendingNotifications = new();
    private readonly MutationTripwire<TState> _tripwire = new();
    private Snapshot _current;
    private bool _isNotifying;

    public TState Current => Volatile.Read(ref _current).Value;

    public IObservable<TState> Changes => _subject;

    public StateStore(TState initialState)
    {
        _current = new(initialState);
        _subject = new(initialState);

        if (StateStoreDiagnostics.DetectMutations)
            _tripwire.Snapshot(initialState);
    }

    public virtual void Update(Func<TState, TState> reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);

        if (ReducerScope.IsActive)
            throw new InvalidOperationException("A state store cannot be updated from inside a reducer. Reducers must be pure; update stores from a subscriber or after the update instead.");

        var detectMutations = StateStoreDiagnostics.DetectMutations;
        var shouldNotify = false;
        string? changedPath = null;

        lock (_gate)
        {
            var current = _current.Value;

            if (detectMutations)
                changedPath = _tripwire.FindChange(current);
            else
                _tripwire.Reset();

            TState next;
            bool isChanged;

            using (ReducerScope.Enter())
            {
                next = reducer(current);
                isChanged = !EqualityComparer<TState>.Default.Equals(current, next);

                if (isChanged)
                {
                    Volatile.Write(ref _current, new(next));
                    OnReduced(next);
                }
            }

            if (isChanged)
            {
                if (detectMutations)
                    _tripwire.Snapshot(next);

                _pendingNotifications.Enqueue(next);

                // Only one thread delivers notifications at a time; it also delivers states queued meanwhile.
                shouldNotify = !_isNotifying;
                _isNotifying = true;
            }
        }

        if (shouldNotify)
            NotifyPending();

        // Thrown after the update, so the store and its subscribers stay consistent while the bug is reported.
        if (changedPath != null)
        {
            throw new InvalidOperationException(
                $"The {typeof(TState).Name} state was changed without Update: '{changedPath}' differs from the last published state. " +
                "Something changed an object reachable from Current or State (for example a @bind on store state) instead of editing a copy. " +
                "This update was applied. The check runs because StateStoreDiagnostics.DetectMutations is on.");
        }
    }

    /// <summary>
    /// Called under the store's lock right after a new state is set, in update order.
    /// Must not block or update stores.
    /// </summary>
    protected virtual void OnReduced(TState newState)
    {
    }

    private void NotifyPending()
    {
        while (true)
        {
            TState next;

            lock (_gate)
            {
                if (!_pendingNotifications.TryDequeue(out next!))
                {
                    _isNotifying = false;
                    return;
                }
            }

            try
            {
                _subject.OnNext(next);
            }
            catch
            {
                // A throwing subscriber must not leave the store stuck; states still queued are delivered by the next update.
                lock (_gate)
                    _isNotifying = false;

                throw;
            }
        }
    }

    public virtual void Dispose()
    {
        _subject.Dispose();
    }
}
