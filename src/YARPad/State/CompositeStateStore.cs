using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace CodingCell.YARPad;

public class CompositeStateStore<TState> : StateStore<TState>
{
    private interface IStateStoreAccessor
    {
        Type StateType { get; }
        object Current { get; }
        IObservable<object> Changes { get; }
    }

    private sealed class StateStoreAccessor<T> : IStateStoreAccessor
    {
        private readonly IStateStore<T> _store;

        public StateStoreAccessor(IStateStore<T> store)
        {
            _store = store;
        }

        public Type StateType => typeof(T);
        public object Current => _store.Current!;
        public IObservable<object> Changes => _store.Changes.Select(x => (object)x!);
    }

    private readonly OrderedDictionary<Type, IStateStoreAccessor> _stateStores;
    private readonly CompositeDisposable _subscriptions;
    private readonly ConstructorInfo _compositeStateConstructor;

    public CompositeStateStore(TState initialState, params List<object> stateStores)
        : base(initialState)
    {
        _subscriptions = new();
        _stateStores = [];

        _compositeStateConstructor = typeof(TState).GetConstructors().First();

        var stateProperties = typeof(TState).GetProperties();
        var ctorParametersOrder = _compositeStateConstructor.GetParameters()
            .Select((x, i) => new { Index = i, Parameter = x.ParameterType })
            .ToDictionary(x => x.Parameter, x => x.Index);

        foreach (var accessor in stateStores.ConvertAll(CreateAccessorForStore).OrderBy(x => ctorParametersOrder[x.StateType]))
            _stateStores.Add(accessor.StateType, accessor);

        foreach (var accessor in _stateStores.Values)
            SubscribeToChildStore(accessor);
    }

    private TState ComposeState()
    {
        var states = _stateStores
            .Select(x => x.Value.Current)
            .ToArray();

        return (TState)_compositeStateConstructor.Invoke(states)!;
    }

    private void SubscribeToChildStore(IStateStoreAccessor accessor)
    {
        var subscription = accessor.Changes.Subscribe(_ => OnChildStateChanged());
        _subscriptions.Add(subscription);
    }

    private void OnChildStateChanged()
    {
        try
        {
            var newState = ComposeState();
            if (!EqualityComparer<TState>.Default.Equals(Current, newState))
            {
                Update(_ => newState);
            }
        }
        catch
        {
            // Fallback: update failed, maintain current state
        }
    }

    public override void Dispose()
    {
        _subscriptions.Dispose();
        base.Dispose();
    }

    private static IStateStoreAccessor CreateAccessor<T>(IStateStore<T> store)
        => new StateStoreAccessor<T>(store);

    private static IStateStoreAccessor CreateAccessorForStore(object store)
    {
        var @interface = store.GetType().GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IStateStore<>));
        var stateType = @interface.GetGenericArguments()[0];

        var method = typeof(CompositeStateStore<TState>).GetMethod(
            nameof(CreateAccessor),
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var genericMethod = method.MakeGenericMethod(stateType);
        return (IStateStoreAccessor)genericMethod.Invoke(null, new[] { store })!;
    }
}
