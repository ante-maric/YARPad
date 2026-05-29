namespace CodingCell.YARPad;

public interface IState<TState>
{
    TState Current { get; }
}

public interface IStoreReader<TState> : IState<TState>
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
