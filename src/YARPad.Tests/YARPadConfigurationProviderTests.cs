using System.Data.Common;
using System.Reactive.Linq;
using System.Text.Json;
using AutoMapper;
using CodingCell.ReactiveStore;
using CodingCell.YARPad.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public sealed class YARPadConfigurationProviderTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;
    private readonly YARPadConfigurationProvider _provider;
    private readonly StateStore<ConfigurationProfileState> _store = new(new([], null));
    private readonly List<ConfigurationProfileState> _emittedStates = [];
    private readonly Guid _configurationID;
    private readonly UpdateInterceptor _updateInterceptor;

    public YARPadConfigurationProviderTests()
    {
        // The in-memory database lives as long as this connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _updateInterceptor = new(this);

        _services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(x => x.UseSqlite(_connection).AddInterceptors(_updateInterceptor))
            .AddScoped(_ => Mock.Of<IDatabaseMigrationService>(x => x.ApplyMigrationsAsync() == Task.CompletedTask))
            .BuildServiceProvider();

        var configuration = new YARPadConfiguration
        {
            Clusters = [new() { ClusterID = "cluster1" }, new() { ClusterID = "cluster2" }],
            Routes = [new() { RouteID = "route1", ClusterID = "cluster1" }, new() { RouteID = "route2", ClusterID = "cluster2" }],
            Policies = new() { [PolicyType.Cors] = [new() { ID = "cors1", Name = "Cors 1" }] },
            CustomTransforms = [new() { Type = "transform1" }],
        };

        var entity = new YARPadConfigurationEntity
        {
            ID = Guid.NewGuid(),
            Name = "Main",
            ConfigurationJson = JsonSerializer.Serialize(configuration),
            CreatedOn = DateTime.UtcNow,
        };
        _configurationID = entity.ID;

        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
            context.Add(entity);
            context.SaveChanges();
        }

        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        var mapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(AutoMapperProfile).Assembly)).CreateMapper();

        _provider = new YARPadConfigurationProvider(
            scopeFactory,
            new DatabaseMigrationGate(scopeFactory),
            mapper,
            Options.Create(new YARPadOptions()),
            NullLogger<YARPadConfigurationProvider>.Instance);

        // Same wiring as YarpConfigurationCoordinator.
        _provider.ConfigurationProfilesChanged += configurations => _store.Update(_ => configurations.ToState());
        _store.Update(_ => new List<YARPadConfigurationEntity> { entity }.ToState());
        _store.Changes.Skip(1).Subscribe(_emittedStates.Add);
    }

    private YARPadConfiguration StoredConfiguration => _store.Current.Profiles.Single().Configuration;

    public static TheoryData<string, Func<IYARPadConfigurationProvider, Guid, YARPadConfiguration, Task>, Func<YARPadConfiguration, bool>> Operations => new()
    {
        { "add cluster", (p, id, c) => p.SaveClusterAsync(id, null, new() { ClusterID = "cluster3" }), c => c.Clusters.Exists(x => x.ClusterID == "cluster3") },
        { "edit cluster", (p, id, c) => p.SaveClusterAsync(id, "cluster1", new() { ClusterID = "renamed" }), c => c.Clusters.Exists(x => x.ClusterID == "renamed") && c.Routes.Find(x => x.RouteID == "route1")!.ClusterID == "renamed" },
        { "delete cluster", (p, id, c) => p.DeleteClusterAsync(id, c.Clusters[0].DeepClone()), c => c.Clusters.ConvertAll(x => x.ClusterID).SequenceEqual(["cluster2"]) },
        { "add route", (p, id, c) => p.SaveRouteAsync(id, null, new() { RouteID = "route3" }, "route1"), c => c.Routes.ConvertAll(x => x.RouteID).SequenceEqual(["route3", "route1", "route2"]) },
        { "edit route", (p, id, c) => p.SaveRouteAsync(id, "route1", new() { RouteID = "renamed" }, null), c => c.Routes.ConvertAll(x => x.RouteID).SequenceEqual(["route2", "renamed"]) },
        { "delete route", (p, id, c) => p.DeleteRouteAsync(id, c.Routes[0].DeepClone()), c => c.Routes.ConvertAll(x => x.RouteID).SequenceEqual(["route2"]) },
        { "toggle route", (p, id, c) => p.ToggleRouteAsync(id, "route1", false), c => !c.Routes.Find(x => x.RouteID == "route1")!.IsEnabled },
        { "add custom transform", (p, id, c) => p.SaveCustomTransformAsync(id, null, new() { Type = "transform2" }), c => c.CustomTransforms.Count == 2 },
        { "edit custom transform", (p, id, c) => p.SaveCustomTransformAsync(id, "transform1", new() { Type = "renamed" }), c => c.CustomTransforms.Single().Type == "renamed" },
        { "delete custom transform", (p, id, c) => p.DeleteCustomTransformAsync(id, c.CustomTransforms[0].DeepClone()), c => c.CustomTransforms.Count == 0 },
        { "add policy", (p, id, c) => p.SavePolicyAsync(id, null, new() { ID = "cors2", Name = "Cors 2" }, PolicyType.Cors), c => c.Policies[PolicyType.Cors].Count == 2 },
        { "edit policy", (p, id, c) => p.SavePolicyAsync(id, "cors1", new() { ID = "cors1", Name = "Renamed" }, PolicyType.Cors), c => c.Policies[PolicyType.Cors].Single().Name == "Renamed" },
        { "delete policy", (p, id, c) => p.DeletePolicyAsync(id, c.Policies[PolicyType.Cors][0] with { }, PolicyType.Cors), c => c.Policies[PolicyType.Cors].Count == 0 },
    };

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Operation_ShouldEmitNewStateWithChange_AndLeavePreviousStateUntouched(
        string name,
        Func<IYARPadConfigurationProvider, Guid, YARPadConfiguration, Task> operation,
        Func<YARPadConfiguration, bool> isApplied)
    {
        var previousConfiguration = StoredConfiguration;
        var before = JsonSerializer.Serialize(previousConfiguration);

        await operation(_provider, _configurationID, previousConfiguration);

        _emittedStates.Count.ShouldBe(1, name);
        StoredConfiguration.ShouldNotBeSameAs(previousConfiguration, name);
        isApplied(StoredConfiguration).ShouldBeTrue(name);
        JsonSerializer.Serialize(previousConfiguration).ShouldBe(before, name);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Operation_ShouldKeepChangesSavedSinceStateWasRead(
        string name,
        Func<IYARPadConfigurationProvider, Guid, YARPadConfiguration, Task> operation,
        Func<YARPadConfiguration, bool> isApplied)
    {
        var staleConfiguration = StoredConfiguration;

        // Another user or instance saves a change that this store has not seen yet.
        SaveChangeElsewhere();

        await operation(_provider, _configurationID, staleConfiguration);

        isApplied(StoredConfiguration).ShouldBeTrue(name);
        StoredConfiguration.Policies[PolicyType.Timeout].ShouldContain(x => x.ID == "timeout1", name);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Operation_ShouldRetry_WhenSavedElsewhereBetweenLoadAndSave(
        string name,
        Func<IYARPadConfigurationProvider, Guid, YARPadConfiguration, Task> operation,
        Func<YARPadConfiguration, bool> isApplied)
    {
        _updateInterceptor.Interruptions = 1;

        await operation(_provider, _configurationID, StoredConfiguration);

        _updateInterceptor.Interruptions.ShouldBe(0, name);
        _emittedStates.Count.ShouldBe(1, name);
        isApplied(StoredConfiguration).ShouldBeTrue(name);
        StoredConfiguration.Policies[PolicyType.Timeout].ShouldContain(x => x.ID == "timeout1", name);
        StoredVersion.ShouldBe(2, name);
    }

    [Fact]
    public async Task Operation_ShouldGiveUp_WhenEverySaveIsInterrupted()
    {
        var previousConfiguration = StoredConfiguration;
        _updateInterceptor.Interruptions = int.MaxValue;

        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => _provider.ToggleRouteAsync(_configurationID, "route1", false));

        _emittedStates.ShouldBeEmpty();
        StoredConfiguration.ShouldBeSameAs(previousConfiguration);
        StoredVersion.ShouldBe(5);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task Operation_ShouldNotChangeState_WhenSaveFails(
        string name,
        Func<IYARPadConfigurationProvider, Guid, YARPadConfiguration, Task> operation,
        Func<YARPadConfiguration, bool> isApplied)
    {
        var previousConfiguration = StoredConfiguration;
        var before = JsonSerializer.Serialize(previousConfiguration);

        // Saving to a profile that does not exist fails.
        await Should.ThrowAsync<InvalidOperationException>(() => operation(_provider, Guid.NewGuid(), previousConfiguration));

        _emittedStates.ShouldBeEmpty(name);
        StoredConfiguration.ShouldBeSameAs(previousConfiguration, name);
        isApplied(previousConfiguration).ShouldBeFalse(name);
        JsonSerializer.Serialize(previousConfiguration).ShouldBe(before, name);
    }

    private long StoredVersion
    {
        get
        {
            using var scope = _services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().YARPadConfigurations.Single(x => x.ID == _configurationID).Version;
        }
    }

    public static TheoryData<string, Action<YARPadConfiguration>, Func<IYARPadConfigurationProvider, Guid, Task>, Func<YARPadConfiguration, bool>> EditsOfDeletedItems => new()
    {
        { "cluster", c => c.Clusters.RemoveAll(x => x.ClusterID == "cluster1"), (p, id) => p.SaveClusterAsync(id, "cluster1", new() { ClusterID = "renamed" }), c => c.Clusters.Exists(x => x.ClusterID == "renamed") },
        { "route", c => c.Routes.RemoveAll(x => x.RouteID == "route1"), (p, id) => p.SaveRouteAsync(id, "route1", new() { RouteID = "renamed" }, null), c => c.Routes.Exists(x => x.RouteID == "renamed") },
        { "custom transform", c => c.CustomTransforms.RemoveAll(x => x.Type == "transform1"), (p, id) => p.SaveCustomTransformAsync(id, "transform1", new() { Type = "renamed" }), c => c.CustomTransforms.Exists(x => x.Type == "renamed") },
        { "policy", c => c.Policies[PolicyType.Cors].RemoveAll(x => x.ID == "cors1"), (p, id) => p.SavePolicyAsync(id, "cors1", new() { ID = "renamed", Name = "Renamed" }, PolicyType.Cors), c => c.Policies[PolicyType.Cors].Exists(x => x.ID == "renamed") },
    };

    [Theory]
    [MemberData(nameof(EditsOfDeletedItems))]
    public async Task Edit_ShouldSaveItemAsNew_WhenItWasDeletedElsewhere(
        string name,
        Action<YARPadConfiguration> deleteElsewhere,
        Func<IYARPadConfigurationProvider, Guid, Task> edit,
        Func<YARPadConfiguration, bool> isSaved)
    {
        SaveChangeElsewhere(deleteElsewhere);

        await edit(_provider, _configurationID);

        isSaved(StoredConfiguration).ShouldBeTrue(name);
    }

    /// <summary>
    /// Saves a change the way another instance would, bypassing the provider under test.
    /// </summary>
    private void SaveChangeElsewhere()
    {
        SaveChangeElsewhere(configuration => configuration.Policies[PolicyType.Timeout] = [new() { ID = "timeout1", Name = "Timeout 1" }]);
    }

    private void SaveChangeElsewhere(Action<YARPadConfiguration> change)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = context.YARPadConfigurations.Single(x => x.ID == _configurationID);
        var configuration = JsonSerializer.Deserialize<YARPadConfiguration>(entity.ConfigurationJson)!;
        change(configuration);
        entity.ConfigurationJson = JsonSerializer.Serialize(configuration);
        entity.Version++;
        context.SaveChanges();
    }

    /// <summary>
    /// Runs <see cref="SaveChangeElsewhere"/> right before the provider's next bulk update,
    /// i.e. after it loaded the configuration but before it saved it.
    /// </summary>
    private sealed class UpdateInterceptor(YARPadConfigurationProviderTests owner) : DbCommandInterceptor
    {
        public int Interruptions { get; set; }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (Interruptions > 0 && command.CommandText.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                Interruptions--;
                owner.SaveChangeElsewhere();
            }

            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    public void Dispose()
    {
        _store.Dispose();
        _services.Dispose();
        _connection.Dispose();
    }
}
