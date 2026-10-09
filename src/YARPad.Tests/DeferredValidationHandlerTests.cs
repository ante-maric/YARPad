using CodingCell.MudValidation;
using FluentValidation;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class DeferredValidationHandlerTests
{
    private sealed class SampleModel
    {
        public string? First { get; set; }
        public string? Second { get; set; }
    }

    // First's rule waits on a gate the test controls, so the validation stays in flight.
    private sealed class GatedValidator : MudValidator<SampleModel>
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public GatedValidator()
        {
            RuleFor(x => x.First)
                .MustAsync(async (value, cancellationToken) =>
                {
                    await _gate.Task.WaitAsync(cancellationToken);
                    return false;
                })
                    .WithMessage("First is invalid.");
        }
    }

    private const string FORBIDDEN_VALUE_KEY = "ForbiddenValue";

    private sealed class SynchronousValidator : MudValidator<SampleModel>
    {
        public int ValidationCount { get; private set; }

        public SynchronousValidator()
        {
            RuleFor(x => x.First)
                .Must((_, value, context) =>
                    !context.RootContextData.TryGetValue(FORBIDDEN_VALUE_KEY, out var forbidden) || !Equals(forbidden, value))
                    .WithMessage("First is forbidden.");

            RuleFor(x => x.Second)
                .NotEmpty()
                    .WithMessage("Second is required.");
        }

        protected override bool PreValidate(ValidationContext<SampleModel> context, FluentValidation.Results.ValidationResult result)
        {
            ValidationCount++;
            return base.PreValidate(context, result);
        }
    }

    private abstract class ShapeModel
    {
    }

    private sealed class NamedShapeModel : ShapeModel
    {
        public string? Name { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    private sealed class NamedShapeValidator : AbstractValidator<NamedShapeModel>
    {
        public NamedShapeValidator()
        {
            RuleFor(x => x)
                .Must(_ => false)
                    .WithMessage("Shape is invalid.");

            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Name is required.");

            RuleForEach(x => x.Tags)
                .NotEmpty()
                    .WithMessage("Tag is required.");
        }
    }

    // All rules sit behind a root-level inheritance validator, like RouteTransformValidator.
    private sealed class ShapeValidator : MudValidator<ShapeModel>
    {
        public ShapeValidator()
        {
            RuleFor(x => x)
                .SetInheritanceValidator(v => v.Add(new NamedShapeValidator()));
        }
    }

    [Fact]
    public async Task Validation_OutsideValidateAsync_ShouldReturnNoErrors()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        var model = new SampleModel();

        // Act
        var errorsBefore = await handler.Validation(model, nameof(SampleModel.Second));
        await handler.ValidateAsync(model, () => handler.Validation(model, nameof(SampleModel.Second)), () => false);
        var errorsAfter = await handler.Validation(model, nameof(SampleModel.Second));

        // Assert
        errorsBefore.ShouldBeEmpty();
        errorsAfter.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ShouldGiveEachFieldTheErrorsOfItsOwnProperty()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        handler.SetContextData(new Dictionary<string, object?> { [FORBIDDEN_VALUE_KEY] = "taken" });
        var model = new SampleModel { First = "taken" };
        IEnumerable<string> firstErrors = [];
        IEnumerable<string> secondErrors = [];

        // Act
        var isValid = await handler.ValidateAsync(
            model,
            async () =>
            {
                firstErrors = await handler.Validation(model, nameof(SampleModel.First));
                secondErrors = await handler.Validation(model, nameof(SampleModel.Second));
            },
            () => false);

        // Assert
        isValid.ShouldBeFalse();
        firstErrors.ShouldBe(["First is forbidden."]);
        secondErrors.ShouldBe(["Second is required."]);
        handler.UnclaimedErrors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_ShouldValidateTheModelOnce_NoMatterHowManyFieldsAsk()
    {
        // Arrange
        var validator = new SynchronousValidator();
        using var handler = new DeferredValidationHandler<SampleModel>(validator);
        var model = new SampleModel();

        // Act
        await handler.ValidateAsync(
            model,
            async () =>
            {
                await handler.Validation(model, nameof(SampleModel.First));
                await handler.Validation(model, nameof(SampleModel.Second));
                await handler.Validation(model, nameof(SampleModel.Second));
            },
            () => false);

        // Assert
        validator.ValidationCount.ShouldBe(1);
    }

    [Fact]
    public async Task ValidateAsync_WhenNoFieldShowsAnError_ShouldReportItAsUnclaimedAndFail()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        var model = new SampleModel { First = "x" };

        // Act - only First has a field and the form itself reports no error.
        var isValid = await handler.ValidateAsync(
            model,
            () => handler.Validation(model, nameof(SampleModel.First)),
            () => true);

        // Assert
        isValid.ShouldBeFalse();
        handler.UnclaimedErrors.ShouldBe(["Second is required."]);
    }

    [Fact]
    public async Task ValidateAsync_WhenModelIsValid_ShouldSucceedAndClearUnclaimedErrors()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        var model = new SampleModel();
        await handler.ValidateAsync(model, () => Task.CompletedTask, () => true);
        handler.UnclaimedErrors.ShouldNotBeEmpty();

        model.Second = "value";

        // Act
        var isValid = await handler.ValidateAsync(model, () => Task.CompletedTask, () => true);

        // Assert
        isValid.ShouldBeTrue();
        handler.UnclaimedErrors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WhenRulesSitBehindRootLevelRule_ShouldStillGiveEachFieldItsOwnErrors()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<ShapeModel>(new ShapeValidator());
        ShapeModel model = new NamedShapeModel { Tags = [""] };
        IEnumerable<string> nameErrors = [];
        IEnumerable<string> tagsErrors = [];

        // Act
        await handler.ValidateAsync(
            model,
            async () =>
            {
                nameErrors = await handler.Validation(model, nameof(NamedShapeModel.Name));
                tagsErrors = await handler.Validation(model, nameof(NamedShapeModel.Tags));
            },
            () => false);

        // Assert
        nameErrors.ShouldBe(["Name is required."]);
        tagsErrors.ShouldBe(["Tag is required."]);
        handler.UnclaimedErrors.ShouldBe(["Shape is invalid."]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NameSuffix")]
    [InlineData("Nam")]
    public async Task ValidateAsync_WhenFieldIsNotBoundToTheProperty_ShouldLeaveItsErrorsUnclaimed(string propertyName)
    {
        // Arrange
        using var handler = new DeferredValidationHandler<ShapeModel>(new ShapeValidator());
        ShapeModel model = new NamedShapeModel();
        IEnumerable<string> fieldErrors = [];

        // Act
        await handler.ValidateAsync(
            model,
            async () => fieldErrors = await handler.Validation(model, propertyName),
            () => true);

        // Assert
        fieldErrors.ShouldBeEmpty();
        handler.UnclaimedErrors.ShouldBe(["Shape is invalid.", "Name is required."], ignoreOrder: true);
    }

    [Fact]
    public async Task ValidateAsync_ShouldSetIsValidatingOnlyWhileRunning_EvenWhenFormValidationThrows()
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        var model = new SampleModel();
        var wasValidating = false;

        // Act
        await Should.ThrowAsync<InvalidOperationException>(() => handler.ValidateAsync(
            model,
            () =>
            {
                wasValidating = handler.IsValidating;
                throw new InvalidOperationException();
            },
            () => true));

        // Assert
        wasValidating.ShouldBeTrue();
        handler.IsValidating.ShouldBeFalse();
        (await handler.Validation(model, nameof(SampleModel.Second))).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true, "value", true)]
    [InlineData(false, "value", false)]
    [InlineData(true, null, false)]
    public async Task TryValidateAndExecuteAsync_ShouldExecuteOnValidOnlyWhenFormAndModelAreValid(bool isFormValid, string? second, bool expected)
    {
        // Arrange
        using var handler = new DeferredValidationHandler<SampleModel>(new SynchronousValidator());
        var model = new SampleModel { Second = second };
        var onValidExecuted = false;

        // Act
        var result = await handler.TryValidateAndExecuteAsync(
            model,
            () => Task.CompletedTask,
            () => isFormValid,
            () =>
            {
                onValidExecuted = true;
                return Task.CompletedTask;
            });

        // Assert
        result.ShouldBe(expected);
        onValidExecuted.ShouldBe(expected);
        handler.IsValidating.ShouldBeFalse();
    }

    [Fact]
    public async Task Dispose_WhileValidationIsInFlight_ShouldEndItWithoutErrors()
    {
        // Arrange
        var handler = new DeferredValidationHandler<SampleModel>(new GatedValidator());
        var model = new SampleModel();
        IEnumerable<string> firstErrors = ["not assigned"];

        // Act
        var isValid = await handler.ValidateAsync(
            model,
            async () =>
            {
                var firstTask = handler.Validation(model, nameof(SampleModel.First));
                handler.Dispose();
                firstErrors = await firstTask;
            },
            () => true);

        // Assert
        isValid.ShouldBeFalse();
        firstErrors.ShouldBeEmpty();
    }

    [Fact]
    public async Task TryValidateAndExecuteAsync_WhenQueuedBehindValidationThatDisposesHandler_ShouldReturnFalseWithoutThrowing()
    {
        // Arrange
        var handler = new DeferredValidationHandler<SampleModel>(new GatedValidator());
        var model = new SampleModel();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var onValidExecuted = false;

        // Act - a second submit queues behind a validation still in flight (double click), then the dialog
        // closes and disposes the handler before either call has finished.
        var first = handler.ValidateAsync(
            model,
            () =>
            {
                firstStarted.SetResult();
                return Task.CompletedTask;
            },
            () => true);
        await firstStarted.Task;

        var second = handler.TryValidateAndExecuteAsync(
            model,
            () => Task.CompletedTask,
            () => true,
            () =>
            {
                onValidExecuted = true;
                return Task.CompletedTask;
            });

        handler.Dispose();

        // Assert - neither call throws or hangs, and the queued one never validates or executes.
        var timeout = TimeSpan.FromSeconds(5);
        (await first.WaitAsync(timeout, TestContext.Current.CancellationToken)).ShouldBeFalse();
        (await second.WaitAsync(timeout, TestContext.Current.CancellationToken)).ShouldBeFalse();
        onValidExecuted.ShouldBeFalse();
        handler.IsValidating.ShouldBeFalse();
    }
}
