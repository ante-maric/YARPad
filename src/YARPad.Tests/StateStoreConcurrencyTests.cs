using CodingCell.ReactiveStore;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class StateStoreConcurrencyTests
{
    private sealed record ValueState(int Value);

    private sealed record CompositeState(int A, int B);

    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public void Update_FromInsideReducer_Throws()
    {
        var storeA = new StateStore<ValueState>(new(0));
        var storeB = new StateStore<ValueState>(new(0));

        Should.Throw<InvalidOperationException>(() => storeA.Update(x =>
        {
            storeB.Update(_ => new(1));
            return x with { Value = 1 };
        }));

        storeA.Current.ShouldBe(new(0));
        storeB.Current.ShouldBe(new(0));

        // The reducer guard is released after the failed update.
        storeA.Update(_ => new(2));
        storeA.Current.ShouldBe(new(2));
    }

    [Fact]
    public void Update_FromSubscriber_IsDeliveredInOrderAfterCurrentNotification()
    {
        var store = new StateStore<ValueState>(new(0));
        var events = new List<string>();

        using var first = store.Changes.Subscribe(x =>
        {
            events.Add($"first:{x.Value}");
            if (x.Value == 1)
                store.Update(_ => new(2));
        });
        using var second = store.Changes.Subscribe(x => events.Add($"second:{x.Value}"));
        events.Clear();

        store.Update(_ => new(1));

        // Not recursive: every subscriber sees 1 before anyone sees 2.
        events.ShouldBe(["first:1", "second:1", "first:2", "second:2"]);
        store.Current.ShouldBe(new(2));
    }

    [Fact]
    public void Update_WhenSubscriberThrows_StoreKeepsWorking()
    {
        var store = new StateStore<ValueState>(new(0));
        var received = new List<int>();
        var shouldThrow = true;

        using var subscription = store.Changes.Subscribe(x =>
        {
            if (x.Value == 1 && shouldThrow)
                throw new InvalidOperationException();

            received.Add(x.Value);
        });
        received.Clear();

        Should.Throw<InvalidOperationException>(() => store.Update(_ => new(1)));
        shouldThrow = false;
        store.Update(_ => new(2));

        received.ShouldBe([2]);
        store.Current.ShouldBe(new(2));
    }

    [Fact]
    public async Task CompositeSubscriberUpdatingChild_WhileChildUpdatedOnOtherThread_DoesNotDeadlock()
    {
        // Composite subscriber updates child B (composite -> child), while another thread updates child A,
        // whose notification recomposes the composite (child -> composite). With locks held during
        // notifications these two lock orders deadlock.
        var storeA = new StateStore<ValueState>(new(0));
        var storeB = new StateStore<ValueState>(new(0));
        using var composite = new CompositeStateStore<CompositeState>(
            new(0, 0),
            () => new(storeA.Current.Value, storeB.Current.Value),
            [storeA, storeB]);

        using var subscription = composite.Changes.Subscribe(x => storeB.Update(_ => new(x.A)));

        const int ITERATIONS = 2_000;
        var writerA = Task.Run(() =>
        {
            for (var i = 1; i <= ITERATIONS; i++)
                storeA.Update(_ => new(i));
        });
        var writerB = Task.Run(() =>
        {
            for (var i = 1; i <= ITERATIONS; i++)
                storeB.Update(_ => new(-i));
        });

        await Task.WhenAll(writerA, writerB).WaitAsync(_timeout, TestContext.Current.CancellationToken);

        // Settle: one more update after both writers finished makes the subscriber copy A into B.
        storeA.Update(_ => new(ITERATIONS + 1));
        composite.Current.ShouldBe(new(ITERATIONS + 1, ITERATIONS + 1));
    }

    [Fact]
    public async Task Composite_UnderConcurrentChildUpdates_EndsWithLatestChildStates()
    {
        var storeA = new StateStore<ValueState>(new(0));
        var storeB = new StateStore<ValueState>(new(0));
        using var composite = new CompositeStateStore<CompositeState>(
            new(0, 0),
            () => new(storeA.Current.Value, storeB.Current.Value),
            [storeA, storeB]);

        const int ITERATIONS = 5_000;
        var writerA = Task.Run(() =>
        {
            for (var i = 1; i <= ITERATIONS; i++)
                storeA.Update(_ => new(i));
        });
        var writerB = Task.Run(() =>
        {
            for (var i = 1; i <= ITERATIONS; i++)
                storeB.Update(_ => new(i));
        });

        await Task.WhenAll(writerA, writerB).WaitAsync(_timeout, TestContext.Current.CancellationToken);

        composite.Current.ShouldBe(new(ITERATIONS, ITERATIONS));
    }

    [Fact]
    public async Task Changes_UnderConcurrentUpdates_AreDeliveredOneAtATimeInUpdateOrder()
    {
        var store = new StateStore<ValueState>(new(0));
        var received = new List<int>();
        var concurrentDeliveries = 0;
        var maxConcurrentDeliveries = 0;

        using var subscription = store.Changes.Subscribe(x =>
        {
            var active = Interlocked.Increment(ref concurrentDeliveries);
            maxConcurrentDeliveries = Math.Max(maxConcurrentDeliveries, active);
            received.Add(x.Value);
            Interlocked.Decrement(ref concurrentDeliveries);
        });
        received.Clear();

        const int WRITERS = 4;
        const int ITERATIONS = 2_000;
        var writers = Enumerable.Range(0, WRITERS).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < ITERATIONS; i++)
                store.Update(x => x with { Value = x.Value + 1 });
        }));

        await Task.WhenAll(writers).WaitAsync(_timeout, TestContext.Current.CancellationToken);

        maxConcurrentDeliveries.ShouldBe(1);
        received.ShouldBe(Enumerable.Range(1, WRITERS * ITERATIONS));
        store.Current.ShouldBe(new(WRITERS * ITERATIONS));
    }
}
