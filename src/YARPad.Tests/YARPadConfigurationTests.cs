using System.Text.Json;
using AutoFixture;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class YARPadConfigurationTests : AutoMapperTest
{
    [Fact]
    public void MapToYARPadConfiguration_ShouldMapAllValues()
    {
        var expected = _fixture.Create<YARPadConfiguration>();

        var actual = _mapper.Map<YARPadConfiguration>(expected);

        expected.Routes.ShouldNotBeEmpty();
        expected.Clusters.ShouldNotBeEmpty();
        expected.Policies.ShouldNotBeEmpty();
        expected.CustomTransforms.ShouldNotBeEmpty();
        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
    }

    [Fact]
    public void MapToYARPadConfiguration_ShouldCreateDeepCopy()
    {
        var expected = _fixture.Create<YARPadConfiguration>();

        var actual = _mapper.Map<YARPadConfiguration>(expected);

        actual.ShouldNotBeSameAs(expected);
        actual.Routes.ShouldNotBeSameAs(expected.Routes);
        actual.Routes[0].ShouldNotBeSameAs(expected.Routes[0]);
        actual.Routes[0].Match.ShouldNotBeSameAs(expected.Routes[0].Match);
        actual.Clusters.ShouldNotBeSameAs(expected.Clusters);
        actual.Clusters[0].ShouldNotBeSameAs(expected.Clusters[0]);
        actual.Clusters[0].Destinations.ShouldNotBeSameAs(expected.Clusters[0].Destinations);
        actual.Policies.ShouldNotBeSameAs(expected.Policies);
        actual.CustomTransforms.ShouldNotBeSameAs(expected.CustomTransforms);
    }
}
