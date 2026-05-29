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
        actual.Clusters.Count.ShouldBe(expected.Clusters.Count);
        
        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
    }
}
