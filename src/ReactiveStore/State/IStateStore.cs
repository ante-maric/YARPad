namespace CodingCell.ReactiveStore;

public interface IState<out TState>
{
    TState Current { get; }
}

public interface IStoreReader<out TState> : IState<TState>
{
    IObservable<TState> Changes { get; }
}

public interface IStoreWriter<TState> : IState<TState>
{
    void Update(Func<TState, TState> reducer);
}

public interface IStateStore<TState> : IStoreReader<TState>, IStoreWriter<TState>, IDisposable
{
}
