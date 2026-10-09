using CodingCell.ReactiveStore;
using Shouldly;

namespace CodingCell.YARPad.Tests;

/// <summary>
/// <see cref="StateStoreDiagnostics.DetectMutations"/> is global, so these tests must not run alongside others.
/// </summary>
[CollectionDefinition(nameof(StateStoreDiagnostics), DisableParallelization = true)]
public class StateStoreDiagnosticsCollection;

[Collection(nameof(StateStoreDiagnostics))]
public sealed class StateStoreMutationTripwireTests : IDisposable
{
    public StateStoreMutationTripwireTests()
    {
        StateStoreDiagnostics.DetectMutations = true;
    }

    public void Dispose()
    {
        StateStoreDiagnostics.DetectMutations = false;
    }

    [Fact]
    public void Update_ShouldNotThrow_WhenDetectionIsOff()
    {
        StateStoreDiagnostics.DetectMutations = false;
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }]));

        store.Current.Items[0].Name = "changed";

        store.Update(x => x with { Version = 1 });
        store.Current.Version.ShouldBe(1);
    }

    [Fact]
    public void Update_ShouldStartCheckingFromTheNextUpdate_WhenDetectionIsTurnedOnLater()
    {
        StateStoreDiagnostics.DetectMutations = false;
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }]));
        store.Current.Items[0].Name = "changed while off";

        StateStoreDiagnostics.DetectMutations = true;

        // The first update only takes the snapshot, so the change made while detection was off is not reported.
        store.Update(x => x with { Version = 1 });

        store.Current.Items[0].Name = "changed while on";
        Should.Throw<InvalidOperationException>(() => store.Update(x => x with { Version = 2 })).Message.ShouldContain("'$.Items[0].Name'");
    }

    private sealed class Item
    {
        public string Name { get; set; } = "";
    }

    private sealed record ItemsState(List<Item> Items, int Version = 0);

    private sealed record UnserializableState(Action Callback, int Version = 0);

    [Fact]
    public void Update_ShouldNotThrow_WhenStateIsOnlyChangedThroughUpdate()
    {
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }]));

        store.Update(x => x with { Items = [.. x.Items, new() { Name = "b" }] });
        store.Update(x => x with { Version = 1 });

        store.Current.Items.Count.ShouldBe(2);
    }

    [Fact]
    public void Update_ShouldThrowWithChangedPath_AfterApplyingTheUpdate_WhenStateWasChangedOutsideUpdate()
    {
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }, new() { Name = "b" }]));
        var emitted = new List<ItemsState>();
        using var subscription = store.Changes.Subscribe(emitted.Add);

        store.Current.Items[1].Name = "changed";

        var exception = Should.Throw<InvalidOperationException>(() => store.Update(x => x with { Version = 1 }));

        exception.Message.ShouldContain("'$.Items[1].Name'");
        store.Current.Version.ShouldBe(1);
        emitted[^1].Version.ShouldBe(1);
    }

    [Fact]
    public void Update_ShouldThrow_WhenStateWasChangedOutsideUpdate_EvenIfTheReducerChangesNothing()
    {
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }]));

        store.Current.Items.Add(new() { Name = "added" });

        Should.Throw<InvalidOperationException>(() => store.Update(x => x)).Message.ShouldContain("'$.Items'");
    }

    [Fact]
    public void Update_ShouldReportAChangeOnce()
    {
        var store = new StateStore<ItemsState>(new([new() { Name = "a" }]));

        store.Current.Items[0].Name = "changed";

        Should.Throw<InvalidOperationException>(() => store.Update(x => x with { Version = 1 }));
        store.Update(x => x with { Version = 2 });

        store.Current.Version.ShouldBe(2);
    }

    [Fact]
    public void Update_ShouldNotThrow_WhenStateCannotBeSerialized()
    {
        var store = new StateStore<UnserializableState>(new(() => { }));

        store.Update(x => x with { Version = 1 });

        store.Current.Version.ShouldBe(1);
    }
}
