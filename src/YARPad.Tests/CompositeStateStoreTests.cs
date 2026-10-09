using CodingCell.ReactiveStore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class CompositeStateStoreTests
{
    private sealed record ValueState(int Value);

    public sealed record CompositeState(int A, int B);

    [Fact]
    public void Current_AfterChildStoreUpdate_ReflectsComposedValuesFromChildStores()
    {
        // Arrange
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var storeB = new StateStore<ValueState>(new ValueState(2));
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(storeA.Current.Value, storeB.Current.Value),
            () => new CompositeState(storeA.Current.Value, storeB.Current.Value),
            [storeA, storeB]);

        // Act
        storeA.Update(_ => new ValueState(10));

        // Assert
        composite.Current.ShouldBe(new CompositeState(10, 2));
    }

    [Fact]
    public void Current_WhenComposerThrows_RetainsPreviousStateAndLogsError()
    {
        // Arrange
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var shouldThrow = false;
        var logger = new Mock<ILogger<CompositeStateStore<CompositeState>>>();
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(storeA.Current.Value, 0),
            () => shouldThrow
                ? throw new InvalidOperationException("composition failed")
                : new CompositeState(storeA.Current.Value, 0),
            [storeA],
            logger.Object);
        var previousState = composite.Current;

        // Act
        shouldThrow = true;
        storeA.Update(_ => new ValueState(99));

        // Assert
        composite.Current.ShouldBe(previousState);
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Batch_WithMultipleChildStoreUpdates_EmitsComposedStateExactlyOnce()
    {
        // Arrange
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var storeB = new StateStore<ValueState>(new ValueState(2));
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(storeA.Current.Value, storeB.Current.Value),
            () => new CompositeState(storeA.Current.Value, storeB.Current.Value),
            [storeA, storeB]);

        var emissionCount = 0;
        using var subscription = composite.Changes.Subscribe(_ => emissionCount++);
        emissionCount = 0; // ignore the initial replay emission from the BehaviorSubject

        // Act
        composite.Batch(() =>
        {
            storeA.Update(_ => new ValueState(10));
            storeB.Update(_ => new ValueState(20));
        });

        // Assert
        emissionCount.ShouldBe(1);
        composite.Current.ShouldBe(new CompositeState(10, 20));
    }

    [Fact]
    public void Batch_WhenNested_RecomposesOnlyAfterOutermostBatch()
    {
        // Arrange
        var (storeA, storeB, composite) = CreateStores();
        var emissions = new List<CompositeState>();
        using var subscription = composite.Changes.Subscribe(emissions.Add);
        emissions.Clear();
        CompositeState? stateAfterInnerBatch = null;

        // Act
        composite.Batch(() =>
        {
            composite.Batch(() => storeA.Update(_ => new ValueState(10)));
            stateAfterInnerBatch = composite.Current;
            storeB.Update(_ => new ValueState(20));
        });

        // Assert
        stateAfterInnerBatch.ShouldBe(new CompositeState(1, 2));
        emissions.ShouldBe([new CompositeState(10, 20)]);
    }

    [Fact]
    public void Batch_WhenUpdatesThrow_StillRecomposesAppliedChanges()
    {
        // Arrange
        var (storeA, _, composite) = CreateStores();

        // Act
        Should.Throw<InvalidOperationException>(() => composite.Batch(() =>
        {
            storeA.Update(_ => new ValueState(10));
            throw new InvalidOperationException();
        }));

        // Assert
        composite.Current.ShouldBe(new CompositeState(10, 2));

        // A failed batch must not leave the store stuck in batching mode.
        storeA.Update(_ => new ValueState(11));
        composite.Current.ShouldBe(new CompositeState(11, 2));
    }

    [Fact]
    public void Batch_WithoutChildStoreUpdates_DoesNotEmit()
    {
        // Arrange
        var (_, _, composite) = CreateStores();
        var emissionCount = 0;
        using var subscription = composite.Changes.Subscribe(_ => emissionCount++);
        emissionCount = 0;

        // Act
        composite.Batch(() => { });

        // Assert
        emissionCount.ShouldBe(0);
    }

    [Fact]
    public void Changes_WhenComposedStateIsUnchanged_DoesNotEmit()
    {
        // Arrange
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(0, 0),
            () => new CompositeState(0, 0),
            [storeA]);
        var emissionCount = 0;
        using var subscription = composite.Changes.Subscribe(_ => emissionCount++);
        emissionCount = 0;

        // Act
        storeA.Update(_ => new ValueState(2));

        // Assert
        emissionCount.ShouldBe(0);
    }

    [Fact]
    public void Dispose_StopsListeningToChildStores()
    {
        // Arrange
        var composerCalls = 0;
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(1, 0),
            () =>
            {
                composerCalls++;
                return new CompositeState(storeA.Current.Value, 0);
            },
            [storeA]);
        composerCalls = 0; // ignore compositions triggered by the initial replay emission

        // Act
        composite.Dispose();
        storeA.Update(_ => new ValueState(2));

        // Assert
        composerCalls.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithNullArguments_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new CompositeStateStore<CompositeState>(new CompositeState(0, 0), null!, []));
        Should.Throw<ArgumentNullException>(() => new CompositeStateStore<CompositeState>(new CompositeState(0, 0), () => new CompositeState(0, 0), null!));
    }

    private static (StateStore<ValueState> StoreA, StateStore<ValueState> StoreB, CompositeStateStore<CompositeState> Composite) CreateStores()
    {
        var storeA = new StateStore<ValueState>(new ValueState(1));
        var storeB = new StateStore<ValueState>(new ValueState(2));
        var composite = new CompositeStateStore<CompositeState>(
            new CompositeState(storeA.Current.Value, storeB.Current.Value),
            () => new CompositeState(storeA.Current.Value, storeB.Current.Value),
            [storeA, storeB]);

        return (storeA, storeB, composite);
    }
}
