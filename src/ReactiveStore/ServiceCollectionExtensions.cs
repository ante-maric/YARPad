using Microsoft.Extensions.DependencyInjection;

namespace CodingCell.ReactiveStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStateStore<TState, TStore>(
        this IServiceCollection services,
        TState initialState,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TState : class
        where TStore : class, IStoreReader<TState>, IStoreWriter<TState>, IStateStore<TState>
    {
        services.Add(new ServiceDescriptor(typeof(TState), sp => initialState, lifetime));
        services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreReader<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreWriter<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStateStore<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));

        return services;
    }

    public static IServiceCollection AddStateStore<TState, TStore>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TState : class
        where TStore : class, IStoreReader<TState>, IStoreWriter<TState>, IStateStore<TState>
    {
        services.Add(new ServiceDescriptor(typeof(TState), typeof(TState), lifetime));
        services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreReader<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStoreWriter<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));
        services.Add(new ServiceDescriptor(typeof(IStateStore<TState>), sp => sp.GetRequiredService<TStore>(), lifetime));

        return services;
    }
}
