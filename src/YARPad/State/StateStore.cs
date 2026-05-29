using System.Reactive.Subjects;

namespace CodingCell.YARPad;

public class StateStore<TState> : IStateStore<TState>
{
    private readonly object _gate = new();

    protected readonly BehaviorSubject<TState> _subject;

    public TState Current => _subject.Value;

    public IObservable<TState> Changes => _subject;

    public StateStore(TState initialState)
    {
        _subject = new(initialState);
    }

    public virtual void Update(Func<TState, TState> reducer)
    {
        lock (_gate)
        {
            var current = _subject.Value;
            var next = reducer(current);

            if (!EqualityComparer<TState>.Default.Equals(current, next))
            {
                OnReduced(next);
                _subject.OnNext(next);
            }
        }
    }

    protected virtual void OnReduced(TState newState)
    {
    }

    public virtual void Dispose()
    {
        _subject.Dispose();
    }
}
