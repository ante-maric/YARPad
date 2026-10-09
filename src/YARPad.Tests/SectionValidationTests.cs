using CodingCell.ReactiveStore;
using CodingCell.MudValidation;
using FluentValidation;
using Moq;
using Shouldly;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodingCell.YARPad.Tests;

public class SectionValidationTests : AutoMapperTest
{
    private static readonly Guid ProfileWithClusterID = Guid.NewGuid();
    private static readonly Guid ProfileWithoutClusterID = Guid.NewGuid();

    private readonly ClusterValidator _clusterValidator;

    public SectionValidationTests()
    {
        var policyValidatorFactory = new Mock<IPolicyValidatorFactory>();
        policyValidatorFactory
            .Setup(x => x.GetValidator(It.IsAny<PolicyType>()))
            .Returns(new MudValidator<PolicyInfo>());

        var factory = policyValidatorFactory.Object;

        _clusterValidator = new ClusterValidator(
            new StateStore<ConfigurationProfileState>(new(
                [
                    new() { ID = ProfileWithClusterID, Name = "With cluster", Configuration = new() { Clusters = [new() { ClusterID = "cluster" }] } },
                    new() { ID = ProfileWithoutClusterID, Name = "Without cluster", Configuration = new() },
                ],
                null)),
            factory,
            new DestinationValidator(),
            new SessionAffinityValidator(factory, new SessionAffinityCookieValidator()),
            new HealthCheckValidator(factory, new ActiveHealthCheckValidator(factory), new PassiveHealthCheckValidator(factory)),
            new ForwarderRequestValidator(),
            new HttpClientValidator(new WebProxyValidator()),
            new ClusterMetadataValidator(Array.Empty<ITransformProvider>(), new YarpMetadataValidator(), _mapper, Mock.Of<IServiceProvider>()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClusterValidator_ShouldValidateOptionalSections_OnlyWhenEnabled(bool isSectionEnabled)
    {
        var cluster = new ClusterModel { ClusterID = "cluster" };
        cluster.HttpClient.RequestHeaderEncoding = "not-an-encoding";
        cluster.SessionAffinity.Policy = "HashCookie";
        cluster.SessionAffinity.AffinityKeyName = string.Empty;
        cluster.SectionSwitches[ClusterConfigSection.HttpClient].IsEnabled = isSectionEnabled;
        cluster.SectionSwitches[ClusterConfigSection.SessionAffinity].IsEnabled = isSectionEnabled;

        var result = await _clusterValidator.ValidateAsync(cluster, TestContext.Current.CancellationToken);

        result.Errors.Any(x => x.PropertyName.StartsWith(nameof(ClusterModel.HttpClient), StringComparison.Ordinal)).ShouldBe(isSectionEnabled);
        result.Errors.Any(x => x.PropertyName.StartsWith(nameof(ClusterModel.SessionAffinity), StringComparison.Ordinal)).ShouldBe(isSectionEnabled);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ClusterValidator_ShouldCheckUniqueness_InTheProfileBeingEdited(bool editsProfileWithCluster, bool expectedIsValid)
    {
        var cluster = new ClusterModel { ClusterID = "cluster" };
        var contextData = ValidatorContextData.CreateCluster(null, editsProfileWithCluster ? ProfileWithClusterID : ProfileWithoutClusterID);

        var result = await _clusterValidator.ValidateFieldAsync(cluster, nameof(ClusterModel.ClusterID), contextData, TestContext.Current.CancellationToken);

        result.Any().ShouldBe(!expectedIsValid);
    }

    [Fact]
    public async Task ClusterValidator_ShouldReportListItemErrorsUnderTheListProperty()
    {
        // The editors bind one field to the whole list, which shows the errors of the list and its items.
        var cluster = new ClusterModel
        {
            ClusterID = "cluster",
            Destinations = [new() { ID = "", Address = "not-a-uri" }],
            Metadata = [new() { Key = "", Value = "value" }],
        };
        cluster.SectionSwitches[ClusterConfigSection.Metadata].IsEnabled = true;

        var result = await _clusterValidator.ValidateAsync(cluster, TestContext.Current.CancellationToken);

        // ClusterID fails too: the test store has no profile to check its uniqueness against.
        var propertyNames = result.Errors.Select(x => x.PropertyName).Where(x => x != nameof(ClusterModel.ClusterID)).ToList();
        propertyNames.ShouldContain(x => x.StartsWith("Destinations[0].", StringComparison.Ordinal));
        propertyNames.ShouldContain(x => x.StartsWith("Metadata[", StringComparison.Ordinal));
        propertyNames.ShouldAllBe(x =>
            x.StartsWith("Destinations[", StringComparison.Ordinal) || x == "Metadata" || x.StartsWith("Metadata[", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ClusterValidator_WhenMetadataKeysAreDuplicated_ShouldReportItInsteadOfThrowing()
    {
        var cluster = new ClusterModel
        {
            ClusterID = "cluster",
            Metadata = [new() { Key = "key", Value = "value" }, new() { Key = "key", Value = "other" }],
        };
        cluster.SectionSwitches[ClusterConfigSection.Metadata].IsEnabled = true;

        var result = await _clusterValidator.ValidateAsync(cluster, TestContext.Current.CancellationToken);

        result.Errors.Select(x => x.ErrorMessage).ShouldContain("Metadata must have unique keys (case-sensitive).");
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task DestinationValidator_ShouldEnforceUniqueID_OnlyWhenEditingDestination(bool isEditingDestination, bool expectedIsValid)
    {
        var cluster = new ClusterModel
        {
            ClusterID = "cluster",
            Destinations = [new() { ID = "existing", Address = "https://localhost" }],
        };
        var destination = new DestinationModel { ID = "existing", Address = "https://localhost" };

        var contextData = new Dictionary<string, object?>();
        contextData.SetClusterModel(cluster);
        contextData.SetIsEditingDestination(isEditingDestination);

        var result = await new DestinationValidator().ValidateFieldAsync(destination, nameof(DestinationModel.ID), contextData, TestContext.Current.CancellationToken);

        result.Any().ShouldBe(!expectedIsValid);
    }
}
