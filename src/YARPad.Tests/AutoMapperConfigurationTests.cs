using AutoFixture;
using AutoFixture.Kernel;
using AutoMapper;

namespace CodingCell.YARPad.Tests;

public class AutoMapperTest
{
    protected readonly IMapper _mapper;
    protected readonly Fixture _fixture;

    public AutoMapperTest()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(AutoMapperProfile).Assembly);
        });

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();

        _fixture = new Fixture();
        _fixture.Customize<RouteModel>(composer =>
        {
            var context = new SpecimenContext(_fixture);

            return composer
                .With(x => x.SectionSwitches,
                    Enum.GetValues<RouteConfigSection>()
                        .Select(s => new RouteConfigSectionSwitch
                        {
                            Section = s,
                            IsEnabled = s == RouteConfigSection.General || s == RouteConfigSection.Match
                        })
                        .ToDictionary(x => x.Section))
                .With(x => x.Transforms,
                    typeof(RouteTransform).Assembly.GetExportedTypes()
                        .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(RouteTransform)))
                        .Select(t => (RouteTransform)context.Resolve(t))
                        .ToList());
        });
    }   
}
