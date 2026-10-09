using Shouldly;

namespace CodingCell.YARPad.Tests;

public class SetOrAppendRulesTests
{
    [Theory]
    [InlineData(null, null, true, false)]   // nothing chosen: Set reports
    [InlineData("", null, true, false)]     // Set chosen in the editor
    [InlineData(null, "", false, true)]     // Append chosen in the editor
    [InlineData("", "", true, false)]       // both empty (imported data): only Set reports
    [InlineData("v", null, true, false)]    // the rule stays on the field in use; NotEmpty then passes
    [InlineData(null, "v", false, true)]
    public void OnlyTheFieldInUseCarriesTheRule(string? set, string? append, bool expectedSetRequired, bool expectedAppendRequired)
    {
        SetOrAppendRules.IsSetRequired(set, append).ShouldBe(expectedSetRequired);
        SetOrAppendRules.IsAppendRequired(set, append).ShouldBe(expectedAppendRequired);
    }

    [Fact]
    public async Task RequestHeaderTransform_WhenSetAndAppendAreBothEmpty_ReportsOnlySet()
    {
        var transform = new RequestHeaderTransform { RequestHeader = "X-Test", Set = "", Append = "" };

        var result = await new RequestHeaderTransformValidator().ValidateAsync(transform, TestContext.Current.CancellationToken);

        result.Errors.Select(x => x.PropertyName).ShouldBe([nameof(RequestHeaderTransform.Set)]);
    }
}
