using CodingCell.MudValidation;
using FluentValidation;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class ValidationContextFactoryTests
{
    private const string CONTEXT_KEY = "Key";

    private sealed record SampleModel(string ScopedValue, string OtherValue);

    private sealed class SampleValidator : AbstractValidator<SampleModel>
    {
        public SampleValidator()
        {
            RuleFor(x => x.ScopedValue)
                .NotEmpty()
                    .WithMessage("ScopedValue is required.");

            RuleFor(x => x.OtherValue)
                .NotEmpty()
                    .WithMessage("OtherValue is required.");

            RuleFor(x => x)
                .Must(x => x.ScopedValue != x.OtherValue)
                    .WithMessage("Values must differ.");
        }
    }

    [Fact]
    public async Task Create_WithPropertyName_ShouldOnlyValidateThatProperty()
    {
        // Arrange
        var model = new SampleModel(ScopedValue: "", OtherValue: "");
        var context = ValidationContextFactory.Create(model, nameof(SampleModel.ScopedValue), new Dictionary<string, object?>());

        // Act
        var result = await new SampleValidator().ValidateAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Errors.Select(e => e.ErrorMessage).ShouldBe(["ScopedValue is required."]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithoutPropertyName_ShouldValidateWholeObject(string? propertyName)
    {
        // Arrange
        var model = new SampleModel(ScopedValue: "", OtherValue: "");
        var context = ValidationContextFactory.Create(model, propertyName, new Dictionary<string, object?>());

        // Act
        var result = await new SampleValidator().ValidateAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.Errors.Select(e => e.ErrorMessage).ShouldBe(
            ["ScopedValue is required.", "OtherValue is required.", "Values must differ."],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Create_WithPropertyName_ShouldNotRunWholeObjectRules()
    {
        // Arrange
        var model = new SampleModel(ScopedValue: "same", OtherValue: "same");
        var context = ValidationContextFactory.Create(model, nameof(SampleModel.ScopedValue), new Dictionary<string, object?>());

        // Act
        var result = await new SampleValidator().ValidateAsync(context, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(nameof(SampleModel.ScopedValue))]
    public void Create_ShouldCopyContextDataIntoRootContextData(string? propertyName)
    {
        // Arrange
        var model = new SampleModel(ScopedValue: "a", OtherValue: "b");
        var contextData = new Dictionary<string, object?> { [CONTEXT_KEY] = 42, ["Null"] = null };

        // Act
        var context = ValidationContextFactory.Create(model, propertyName, contextData);

        // Assert
        context.InstanceToValidate.ShouldBeSameAs(model);
        context.RootContextData[CONTEXT_KEY].ShouldBe(42);
        context.RootContextData.ShouldContainKey("Null");
        context.RootContextData["Null"].ShouldBeNull();
    }
}
