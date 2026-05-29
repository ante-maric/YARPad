using AutoFixture;
using Shouldly;
using Yarp.ReverseProxy.Configuration;
using CodingCell.YARPad;

namespace CodingCell.YARPad.Tests;

public class RouteModelTests : AutoMapperTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapToRouteConfig_ShouldToggleOptionalSections_When(bool isSectionEnabled)
    {
        var expected = _fixture.Create<RouteModel>();

        SetSectionEnabled(expected, isSectionEnabled,
            RouteConfigSection.Transform,
            RouteConfigSection.Metadata);

        var actual = _mapper.Map<RouteConfig>(expected);

        actual.Match.ShouldNotBeNull();
        (actual.Transforms != null).ShouldBe(isSectionEnabled);
        (actual.Metadata != null).ShouldBe(isSectionEnabled);
    }

    [Fact]
    public void MapToRouteConfig_ShouldMapAllValues_WhenOptionalSectionsEnabled()
    {
        var expected = _fixture.Create<RouteModel>();

        SetSectionEnabled(expected, true,
            RouteConfigSection.Transform,
            RouteConfigSection.Metadata);

        var actual = _mapper.Map<RouteConfig>(expected);

        actual.RouteId.ShouldBe(expected.RouteID);
        actual.ClusterId.ShouldBe(expected.ClusterID);
        actual.MaxRequestBodySize.ShouldBe(expected.MaxRequestBodySize);
        actual.AuthorizationPolicy.ShouldBe(expected.AuthorizationPolicy);
        actual.RateLimiterPolicy.ShouldBe(expected.RateLimiterPolicy);
        actual.OutputCachePolicy.ShouldBe(expected.OutputCachePolicy);
        actual.Timeout.ShouldBe(expected.Timeout);
        actual.TimeoutPolicy.ShouldBe(expected.TimeoutPolicy);
        actual.CorsPolicy.ShouldBe(expected.CorsPolicy);

        actual.Match.ShouldNotBeNull();
        var actualMatch = actual.Match;
        actualMatch.Path.ShouldBe(expected.Match.Path);
        actualMatch.Hosts.ShouldBe(expected.Match.Hosts);
        actualMatch.Methods.ShouldBe(expected.Match.Methods);
        actualMatch.Headers.ShouldNotBeNull();
        actualMatch.Headers.Count.ShouldBe(expected.Match.Headers.Count);
        actualMatch.Headers[0].Name.ShouldBe(expected.Match.Headers[0].Name);
        actualMatch.Headers[0].Values.ShouldBe(expected.Match.Headers[0].Values);
        actualMatch.Headers[0].Mode.ShouldBe(expected.Match.Headers[0].Mode);
        actualMatch.Headers[0].IsCaseSensitive.ShouldBe(expected.Match.Headers[0].IsCaseSensitive);
        actualMatch.QueryParameters.ShouldNotBeNull();
        actualMatch.QueryParameters.Count.ShouldBe(expected.Match.QueryParameters.Count);
        actualMatch.QueryParameters[0].Name.ShouldBe(expected.Match.QueryParameters[0].Name);
        actualMatch.QueryParameters[0].Values.ShouldBe(expected.Match.QueryParameters[0].Values);
        actualMatch.QueryParameters[0].Mode.ShouldBe(expected.Match.QueryParameters[0].Mode);
        actualMatch.QueryParameters[0].IsCaseSensitive.ShouldBe(expected.Match.QueryParameters[0].IsCaseSensitive);

        actual.Metadata.ShouldNotBeNull();
        actual.Metadata.ShouldBe(expected.Metadata.ToDictionary(m => m.Key, m => m.Value!));

        AssertTransformsMapped(expected.Transforms, actual.Transforms);
    }

    private static void SetSectionEnabled(RouteModel route, bool enabled, params RouteConfigSection[] sections)
    {
        foreach (var section in sections)
        {
            route.SectionSwitches[section].IsEnabled = enabled;
        }
    }

    private static void AssertTransformsMapped(IReadOnlyList<RouteTransform> expected, IReadOnlyList<IReadOnlyDictionary<string, string>>? actual)
    {
        var expectedDictionaries = expected.Select(x => x.ToDictionary()).ToList();
        var actualTransforms = actual.ShouldNotBeNull().ToList();

        actualTransforms.Count.ShouldBe(expectedDictionaries.Count);

        for (var i = 0; i < expectedDictionaries.Count; i++)
        {
            var expectedDictionary = expectedDictionaries[i];
            var actualDictionary = actualTransforms[i];

            actualDictionary.Count.ShouldBe(expectedDictionary.Count);
            foreach (var kvp in expectedDictionary)
            {
                actualDictionary.ShouldContainKeyAndValue(kvp.Key, kvp.Value);
            }
        }
    }
}
