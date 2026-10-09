using Shouldly;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad.Tests;

public class MatchValuesValidationTests
{
    [Theory]
    [InlineData(HeaderMatchMode.ExactHeader, false)]
    [InlineData(HeaderMatchMode.HeaderPrefix, false)]
    [InlineData(HeaderMatchMode.Contains, false)]
    [InlineData(HeaderMatchMode.NotContains, false)]
    [InlineData(HeaderMatchMode.Exists, true)]
    [InlineData(HeaderMatchMode.NotExists, true)]
    public async Task Header_EmptyValues_RequiredUnlessExistenceMode(HeaderMatchMode mode, bool expectedValid)
    {
        var header = new RouteHeaderModel { Name = "X-Test", Mode = mode };

        var result = await new RouteHeaderValidator().ValidateAsync(header, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBe(expectedValid);
    }

    [Theory]
    [InlineData(HeaderMatchMode.Exists)]
    [InlineData(HeaderMatchMode.NotExists)]
    public async Task Header_ValuesSet_InvalidForExistenceModes(HeaderMatchMode mode)
    {
        var header = new RouteHeaderModel { Name = "X-Test", Mode = mode, Values = ["a"] };

        var result = await new RouteHeaderValidator().ValidateAsync(header, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(QueryParameterMatchMode.Exact, false)]
    [InlineData(QueryParameterMatchMode.Prefix, false)]
    [InlineData(QueryParameterMatchMode.Contains, false)]
    [InlineData(QueryParameterMatchMode.NotContains, false)]
    [InlineData(QueryParameterMatchMode.Exists, true)]
    public async Task QueryParameter_EmptyValues_RequiredUnlessExists(QueryParameterMatchMode mode, bool expectedValid)
    {
        var parameter = new RouteQueryParameterModel { Name = "q", Mode = mode };

        var result = await new RouteQueryParameterValidator().ValidateAsync(parameter, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBe(expectedValid);
    }

    [Fact]
    public async Task QueryParameter_ValuesSet_InvalidForExists()
    {
        var parameter = new RouteQueryParameterModel { Name = "q", Mode = QueryParameterMatchMode.Exists, Values = ["a"] };

        var result = await new RouteQueryParameterValidator().ValidateAsync(parameter, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Metadata_EmptyKey_InvalidOutsideEditingDialog()
    {
        var result = await new YarpMetadataValidator().ValidateAsync(new YarpMetadata { Key = "" }, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Destination_EmptyID_InvalidOutsideEditingDialog()
    {
        var destination = new DestinationModel { ID = "", Address = "https://example.test" };

        var result = await new DestinationValidator().ValidateAsync(destination, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }
}
