using System.Diagnostics;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Yarp.ReverseProxy.Configuration;
using CodingCell.YARPad.Data;

namespace CodingCell.YARPad.Tests;

public class YarpConfigProviderTests
{
    private readonly Mock<IYARPadConfigurationProvider> _configurationProvider;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IConfigValidator> _validator;
    private readonly Mock<ILogger<YarpConfigProvider>> _logger;

    public YarpConfigProviderTests()
    {
        _configurationProvider = new Mock<IYARPadConfigurationProvider>();
        _mapper = new Mock<IMapper>();
        _validator = new Mock<IConfigValidator>();
        _logger = new Mock<ILogger<YarpConfigProvider>>();

        _validator.Setup(v => v.ValidateRouteAsync(It.IsAny<RouteConfig>()))
            .ReturnsAsync([]);
        _validator.Setup(v => v.ValidateClusterAsync(It.IsAny<ClusterConfig>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task GetConfig_FirstCallLoadsConfigurationAndUpdatesCurrentConfig()
    {
        var configId = Guid.NewGuid();
        var yarpadConfig = new YARPadConfiguration();
        var entity = new YARPadConfigurationEntity 
        { 
            ID = configId, 
            Name = "Test", 
            ConfigurationJson = "{}", 
            IsActive = true 
        };
        var mappedConfig = CreateConfig("route-1", "cluster-1");

        _configurationProvider.Setup(p => p.GetConfigurationsAsync())
            .ReturnsAsync([entity]);
        _mapper.Setup(m => m.Map<YarpConfig>(It.IsAny<YARPadConfiguration>())).Returns(mappedConfig);

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        var initialConfig = provider.GetConfig();
        initialConfig.ShouldNotBeNull();

        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mappedConfig));

        initialConfig.ShouldNotBeSameAs(mappedConfig);
        _validator.Verify(v => v.ValidateRouteAsync(It.IsAny<RouteConfig>()), Times.Once);
        _validator.Verify(v => v.ValidateClusterAsync(It.IsAny<ClusterConfig>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_UpdatesCurrentConfig()
    {
        var profile = CreateProfile(Guid.NewGuid(), "route-2", "cluster-2");
        var mappedConfig = CreateConfig("route-2", "cluster-2");

        _mapper.Setup(m => m.Map<YarpConfig>(profile.Configuration)).Returns(mappedConfig);

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        await provider.UpdateConfigurationAsync(profile);

        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mappedConfig));
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WhenMappingFails_DoesNotUpdateConfig()
    {
        var initialProfile = CreateProfile(Guid.NewGuid(), "route-3", "cluster-3");
        var failingProfile = CreateProfile(Guid.NewGuid(), "route-fail", "cluster-fail");
        var mappedInitial = CreateConfig("route-3", "cluster-3");

        _mapper.Setup(m => m.Map<YarpConfig>(initialProfile.Configuration)).Returns(mappedInitial);
        _mapper.Setup(m => m.Map<YarpConfig>(failingProfile.Configuration)).Throws(new InvalidOperationException("boom"));

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        await provider.UpdateConfigurationAsync(initialProfile);
        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mappedInitial));

        await provider.UpdateConfigurationAsync(failingProfile);
        await WaitForLogAsync(_logger, LogLevel.Error, "Failed to map YARPad configuration to YARP config.");

        provider.GetConfig().ShouldBeSameAs(mappedInitial);
        VerifyLog(_logger, LogLevel.Error, "Failed to map YARPad configuration to YARP config.", Times.Once());
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WhenValidationHasErrors_DoesNotUpdateConfig()
    {
        var initialProfile = CreateProfile(Guid.NewGuid(), "route-4", "cluster-4");
        var failingProfile = CreateProfile(Guid.NewGuid(), "route-5", "cluster-5");
        var mappedInitial = CreateConfig("route-4", "cluster-4");
        var mappedFailing = CreateConfig("route-5", "cluster-5");

        _mapper.Setup(m => m.Map<YarpConfig>(initialProfile.Configuration)).Returns(mappedInitial);
        _mapper.Setup(m => m.Map<YarpConfig>(failingProfile.Configuration)).Returns(mappedFailing);

        _validator.SetupSequence(v => v.ValidateRouteAsync(It.IsAny<RouteConfig>()))
            .ReturnsAsync([])
            .ReturnsAsync([new InvalidOperationException("invalid")]);

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        await provider.UpdateConfigurationAsync(initialProfile);
        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mappedInitial));

        await provider.UpdateConfigurationAsync(failingProfile);
        await WaitForLogAsync(_logger, LogLevel.Warning, "YARP route validation error");

        provider.GetConfig().ShouldBeSameAs(mappedInitial);
        VerifyLog(_logger, LogLevel.Warning, "YARP route validation error", Times.AtLeastOnce());
    }

    [Fact]
    public async Task UpdateConfigurationAsync_WhenValidationThrows_DoesNotUpdateConfig()
    {
        var initialProfile = CreateProfile(Guid.NewGuid(), "route-6", "cluster-6");
        var failingProfile = CreateProfile(Guid.NewGuid(), "route-7", "cluster-7");
        var mappedInitial = CreateConfig("route-6", "cluster-6");
        var mappedFailing = CreateConfig("route-7", "cluster-7");

        _mapper.Setup(m => m.Map<YarpConfig>(initialProfile.Configuration)).Returns(mappedInitial);
        _mapper.Setup(m => m.Map<YarpConfig>(failingProfile.Configuration)).Returns(mappedFailing);

        _validator.SetupSequence(v => v.ValidateRouteAsync(It.IsAny<RouteConfig>()))
            .ReturnsAsync([])
            .ThrowsAsync(new InvalidOperationException("validation failed"));

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        await provider.UpdateConfigurationAsync(initialProfile);
        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mappedInitial));

        await provider.UpdateConfigurationAsync(failingProfile);
        await WaitForLogAsync(_logger, LogLevel.Error, "Failed to validate YARP route");

        provider.GetConfig().ShouldBeSameAs(mappedInitial);
        VerifyLog(_logger, LogLevel.Error, "Failed to validate YARP route", Times.Once());
    }

    [Fact]
    public async Task UpdateConfigurationAsync_CoalescesPendingUpdatesWhileRunning()
    {
        var profile1 = CreateProfile(Guid.NewGuid(), "route-8", "cluster-8");
        var profile2 = CreateProfile(Guid.NewGuid(), "route-9", "cluster-9");
        var profile3 = CreateProfile(Guid.NewGuid(), "route-10", "cluster-10");

        var mapped1 = CreateConfig("route-8", "cluster-8");
        var mapped2 = CreateConfig("route-9", "cluster-9");
        var mapped3 = CreateConfig("route-10", "cluster-10");

        _mapper.Setup(m => m.Map<YarpConfig>(profile1.Configuration)).Returns(mapped1);
        _mapper.Setup(m => m.Map<YarpConfig>(profile2.Configuration)).Returns(mapped2);
        _mapper.Setup(m => m.Map<YarpConfig>(profile3.Configuration)).Returns(mapped3);

        var validationStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowFirstValidation = new TaskCompletionSource<IList<Exception>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var validationCount = 0;

        _validator.Setup(v => v.ValidateRouteAsync(It.IsAny<RouteConfig>()))
            .Returns(async () =>
            {
                var callIndex = Interlocked.Increment(ref validationCount);
                if (callIndex == 1)
                {
                    validationStarted.TrySetResult();
                    return await allowFirstValidation.Task;
                }

                return [];
            });

        using var provider = new YarpConfigProvider(_configurationProvider.Object, _mapper.Object, _validator.Object, _logger.Object);

        _ = provider.UpdateConfigurationAsync(profile1);
        await validationStarted.Task;

        _ = provider.UpdateConfigurationAsync(profile2);
        _ = provider.UpdateConfigurationAsync(profile3);

        allowFirstValidation.SetResult([]);

        await WaitForConditionAsync(() => ReferenceEquals(provider.GetConfig(), mapped3));

        _mapper.Verify(m => m.Map<YarpConfig>(profile1.Configuration), Times.Once);
        _mapper.Verify(m => m.Map<YarpConfig>(profile2.Configuration), Times.Never);
        _mapper.Verify(m => m.Map<YarpConfig>(profile3.Configuration), Times.Once);
    }

    private static ConfigurationProfile CreateProfile(Guid id, string routeId, string clusterId)
    {
        return new ConfigurationProfile
        {
            ID = id,
            Name = "Test Profile",
            Configuration = new YARPadConfiguration()
        };
    }

    private static YarpConfig CreateConfig(string routeId, string clusterId)
    {
        return new YarpConfig
        {
            Routes = [new RouteConfig { RouteId = routeId }],
            Clusters = [new ClusterConfig { ClusterId = clusterId }]
        };
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = Stopwatch.StartNew();

        while (start.Elapsed < timeout)
        {
            if (condition())
                return;

            await Task.Delay(50);
        }

        condition().ShouldBeTrue("Timed out waiting for async update.");
    }

    private static Task WaitForLogAsync(
        Mock<ILogger<YarpConfigProvider>> logger,
        LogLevel level,
        string containsMessage)
    {
        return WaitForConditionAsync(() =>
            logger.Invocations.Any(invocation =>
                invocation.Arguments.Count >= 3
                && invocation.Arguments[0] is LogLevel loggedLevel
                && loggedLevel == level
                && invocation.Arguments[2]?.ToString() != null
                && invocation.Arguments[2]!.ToString()!.Contains(containsMessage)));
    }

    private static void VerifyLog(
        Mock<ILogger<YarpConfigProvider>> logger,
        LogLevel level,
        string containsMessage,
        Times times)
    {
        logger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() != null && v.ToString()!.Contains(containsMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
