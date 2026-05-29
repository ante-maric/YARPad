using AutoFixture;
using Shouldly;
using Yarp.ReverseProxy.Configuration;
using CodingCell.YARPad;

namespace CodingCell.YARPad.Tests;

public class ClusterModelTests : AutoMapperTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MapToClusterConfig_ShouldToggleOptionalSections_When(bool isSectionEnabled)
    {
        var expected = CreateClusterModelWithData();

        SetSectionEnabled(expected, isSectionEnabled,
            ClusterConfigSection.Metadata,
            ClusterConfigSection.SessionAffinity,
            ClusterConfigSection.HealthCheck,
            ClusterConfigSection.HttpClient,
            ClusterConfigSection.HttpRequest);

        var actual = _mapper.Map<ClusterConfig>(expected);

        actual.Destinations.ShouldNotBeNull();
        (actual.Metadata != null).ShouldBe(isSectionEnabled);
        (actual.SessionAffinity != null).ShouldBe(isSectionEnabled);
        (actual.HealthCheck != null).ShouldBe(isSectionEnabled);
        (actual.HttpClient != null).ShouldBe(isSectionEnabled);
        (actual.HttpRequest != null).ShouldBe(isSectionEnabled);
    }

    [Fact]
    public void MapToClusterConfig_ShouldMapAllValues_WhenOptionalSectionsEnabled()
    {
        var expected = CreateClusterModelWithData();

        SetSectionEnabled(expected, true,
            ClusterConfigSection.Metadata,
            ClusterConfigSection.SessionAffinity,
            ClusterConfigSection.HealthCheck,
            ClusterConfigSection.HttpClient,
            ClusterConfigSection.HttpRequest);

        var actual = _mapper.Map<ClusterConfig>(expected);

        actual.ClusterId.ShouldBe(expected.ClusterID);
        actual.LoadBalancingPolicy.ShouldBe(expected.LoadBalancingPolicy);

        var expectedDestinations = expected.Destinations.OrderBy(x => x.ID).ToList();
        var actualDestinations = actual.Destinations.ShouldNotBeNull().OrderBy(x => x.Key).ToList();
        actualDestinations.Count.ShouldBe(expectedDestinations.Count);

        for (var i = 0; i < expectedDestinations.Count; i++)
        {
            var expectedDestination = expectedDestinations[i];
            var actualDestination = actualDestinations[i];

            actualDestination.Key.ShouldBe(expectedDestination.ID);
            actualDestination.Value.Address.ShouldBe(expectedDestination.Address);
            actualDestination.Value.Health.ShouldBe(expectedDestination.Health);
            actualDestination.Value.Host.ShouldBe(expectedDestination.Host);
            actualDestination.Value.Metadata.ShouldBe(expectedDestination.Metadata.ToDictionary(x => x.Key, x => x.Value!));
        }

        actual.SessionAffinity.ShouldNotBeNull();
        var sessionAffinity = actual.SessionAffinity;
        sessionAffinity.Enabled.ShouldBe(true);
        sessionAffinity.Policy.ShouldBe(expected.SessionAffinity.Policy);
        sessionAffinity.FailurePolicy.ShouldBe(expected.SessionAffinity.FailurePolicy);
        sessionAffinity.AffinityKeyName.ShouldBe(expected.SessionAffinity.AffinityKeyName);
        sessionAffinity.Cookie.ShouldNotBeNull();
        sessionAffinity.Cookie.Path.ShouldBe(expected.SessionAffinity.Cookie.Path);
        sessionAffinity.Cookie.Domain.ShouldBe(expected.SessionAffinity.Cookie.Domain);
        sessionAffinity.Cookie.HttpOnly.ShouldBe(expected.SessionAffinity.Cookie.HttpOnly);
        sessionAffinity.Cookie.SecurePolicy.ShouldBe(expected.SessionAffinity.Cookie.SecurePolicy);
        sessionAffinity.Cookie.SameSite.ShouldBe(expected.SessionAffinity.Cookie.SameSite);
        sessionAffinity.Cookie.Expiration.ShouldBe(expected.SessionAffinity.Cookie.Expiration);
        sessionAffinity.Cookie.MaxAge.ShouldBe(expected.SessionAffinity.Cookie.MaxAge);
        sessionAffinity.Cookie.IsEssential.ShouldBe(expected.SessionAffinity.Cookie.IsEssential);

        actual.HealthCheck.ShouldNotBeNull();
        var healthCheck = actual.HealthCheck!;
        healthCheck.AvailableDestinationsPolicy.ShouldBe(expected.HealthCheck.AvailableDestinationsPolicy);
        healthCheck.Passive.ShouldNotBeNull();
        healthCheck.Passive.Enabled.ShouldBe(expected.HealthCheck.Passive.Enabled);
        healthCheck.Passive.Policy.ShouldBe(expected.HealthCheck.Passive.Policy);
        healthCheck.Passive.ReactivationPeriod.ShouldBe(expected.HealthCheck.Passive.ReactivationPeriod);
        healthCheck.Active.ShouldNotBeNull();
        healthCheck.Active.Enabled.ShouldBe(expected.HealthCheck.Active.Enabled);
        healthCheck.Active.Interval.ShouldBe(expected.HealthCheck.Active.Interval);
        healthCheck.Active.Timeout.ShouldBe(expected.HealthCheck.Active.Timeout);
        healthCheck.Active.Policy.ShouldBe(expected.HealthCheck.Active.Policy);
        healthCheck.Active.Path.ShouldBe(expected.HealthCheck.Active.Path);
        healthCheck.Active.Query.ShouldBe(expected.HealthCheck.Active.Query);

        actual.HttpClient.ShouldNotBeNull();
        var httpClient = actual.HttpClient!;
        httpClient.SslProtocols.ShouldBe(expected.HttpClient.SslProtocols.ToSingleFlag());
        httpClient.DangerousAcceptAnyServerCertificate.ShouldBe(expected.HttpClient.DangerousAcceptAnyServerCertificate);
        httpClient.MaxConnectionsPerServer.ShouldBe(expected.HttpClient.MaxConnectionsPerServer);
        httpClient.EnableMultipleHttp2Connections.ShouldBe(expected.HttpClient.EnableMultipleHttp2Connections);
        httpClient.RequestHeaderEncoding.ShouldBe(expected.HttpClient.RequestHeaderEncoding);
        httpClient.ResponseHeaderEncoding.ShouldBe(expected.HttpClient.ResponseHeaderEncoding);
        httpClient.WebProxy.ShouldNotBeNull();
        httpClient.WebProxy.Address.ShouldBe(new Uri(expected.HttpClient.WebProxy.Address!));
        httpClient.WebProxy.BypassOnLocal.ShouldBe(expected.HttpClient.WebProxy.BypassOnLocal);
        httpClient.WebProxy.UseDefaultCredentials.ShouldBe(expected.HttpClient.WebProxy.UseDefaultCredentials);

        actual.HttpRequest.ShouldNotBeNull();
        var httpRequest = actual.HttpRequest!;
        httpRequest.ActivityTimeout.ShouldBe(expected.HttpRequest.ActivityTimeout);
        httpRequest.Version.ShouldBe(expected.HttpRequest.Version);
        httpRequest.VersionPolicy.ShouldBe(expected.HttpRequest.VersionPolicy);
        httpRequest.AllowResponseBuffering.ShouldBe(expected.HttpRequest.AllowResponseBuffering);

        actual.Metadata.ShouldNotBeNull();
        actual.Metadata.ShouldBe(expected.Metadata.ToDictionary(m => m.Key, m => m.Value!));
    }

    [Fact]
    public void MapToClusterConfig_ShouldOnlyMapEnabledDestinations()
    {
        var expected = CreateClusterModelWithData();
        expected.Destinations =
        [
            new DestinationModel { ID = "destination1", Address = "https://enabled.test", IsEnabled = true },
            new DestinationModel { ID = "destination2", Address = "https://disabled.test", IsEnabled = false },
        ];

        var actual = _mapper.Map<ClusterConfig>(expected);

        actual.Destinations.ShouldNotBeNull();
        actual.Destinations.Count.ShouldBe(1);
        actual.Destinations.ContainsKey("destination1").ShouldBeTrue();
        actual.Destinations.ContainsKey("destination2").ShouldBeFalse();
    }

    private ClusterModel CreateClusterModelWithData()
    {
        var cluster = _fixture.Build<ClusterModel>()
            .With(x => x.SectionSwitches, Enum.GetValues<ClusterConfigSection>()
                .Select(x => new ClusterConfigSectionSwitch { Section = x, IsEnabled = x == ClusterConfigSection.Destinations })
                .ToDictionary(x => x.Section))
            .With(x => x.HttpClient, _fixture.Build<HttpClientModel>()
                .With(x => x.WebProxy, _fixture.Build<WebProxyModel>()
                    .With(x => x.Address, "https://test.com")
                    .Create())
                .Create())
            .Create();

        foreach (var destination in cluster.Destinations)
        {
            destination.IsEnabled = true;
        }

        return cluster;
    }

    private static void SetSectionEnabled(ClusterModel cluster, bool enabled, params ClusterConfigSection[] sections)
    {
        foreach (var section in sections)
        {
            cluster.SectionSwitches[section].IsEnabled = enabled;
        }
    }
}
