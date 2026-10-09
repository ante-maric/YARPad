using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class LanAccessValidatorTests
{
    [Theory]
    [InlineData("127.0.0.1", true)]  // Loopback IPv4
    [InlineData("127.10.20.30", true)]  // Loopback IPv4 range
    [InlineData("::1", true)]  // Loopback IPv6
    [InlineData("10.0.0.1", true)]  // Private Class A
    [InlineData("10.255.255.255", true)]  // Private Class A upper bound
    [InlineData("172.16.0.1", true)]  // Private Class B lower bound
    [InlineData("172.31.255.255", true)]  // Private Class B upper bound
    [InlineData("172.15.255.255", false)]  // Just below Private Class B
    [InlineData("172.32.0.1", false)]  // Just above Private Class B
    [InlineData("192.168.1.1", true)]  // Private Class C
    [InlineData("192.169.0.1", false)]  // Just above Private Class C
    [InlineData("fc00::1", true)]  // IPv6 ULA
    [InlineData("fdff:ffff::1", true)]  // IPv6 ULA upper half
    [InlineData("fe80::1", true)]  // IPv6 link-local
    [InlineData("::ffff:192.168.1.1", true)]  // Private IPv4 mapped to IPv6
    [InlineData("::ffff:8.8.8.8", false)]  // Public IPv4 mapped to IPv6
    [InlineData("8.8.8.8", false)]  // Public IP (Google DNS)
    [InlineData("1.1.1.1", false)]  // Public IP (Cloudflare DNS)
    [InlineData("2001:4860:4860::8888", false)]  // Public IPv6 (Google DNS)
    public void IsAllowedAddress_WithDefaultSettings_ValidatesCorrectly(string ipString, bool expectedResult)
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions());

        // Act
        var result = validator.IsAllowedAddress(IPAddress.Parse(ipString));

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public void IsAllowedAddress_WithLoopbackDisabled_BlocksLoopback(string ipString)
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions { AllowLoopback = false });

        // Act
        var result = validator.IsAllowedAddress(IPAddress.Parse(ipString));

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("10.0.0.1", false)]
    [InlineData("192.168.1.1", false)]
    [InlineData("fc00::1", false)]
    [InlineData("fe80::1", false)]
    public void IsAllowedAddress_WithDefaultPrivateRangesDisabled_OnlyAllowsLoopback(string ipString, bool expectedResult)
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions { IncludeDefaultPrivateRanges = false });

        // Act
        var result = validator.IsAllowedAddress(IPAddress.Parse(ipString));

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("203.0.113.100", true)]
    [InlineData("203.0.114.100", false)]
    [InlineData("2a02:1234::1", true)]
    [InlineData("2a02:1235::1", false)]
    [InlineData("192.168.1.1", false)]
    public void IsAllowedAddress_WithAdditionalAllowedRanges_AllowsOnlyCustomRanges(string ipString, bool expectedResult)
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions
        {
            AllowLoopback = false,
            IncludeDefaultPrivateRanges = false,
            AdditionalAllowedRanges = ["203.0.113.0/24", "2a02:1234::/32"]
        });

        // Act
        var result = validator.IsAllowedAddress(IPAddress.Parse(ipString));

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void IsAllowedAddress_WithInvalidAdditionalRange_IgnoresItAndKeepsValidRanges()
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions
        {
            AllowLoopback = false,
            IncludeDefaultPrivateRanges = false,
            AdditionalAllowedRanges = ["not a range", "203.0.113.0/24"]
        });

        // Act
        var allowedResult = validator.IsAllowedAddress(IPAddress.Parse("203.0.113.1"));
        var blockedResult = validator.IsAllowedAddress(IPAddress.Parse("8.8.8.8"));

        // Assert
        allowedResult.ShouldBeTrue();
        blockedResult.ShouldBeFalse();
    }

    [Fact]
    public void IsAllowedAddress_WithNothingAllowed_BlocksEverything()
    {
        // Arrange
        var validator = CreateValidator(new LanAccessOptions
        {
            AllowLoopback = false,
            IncludeDefaultPrivateRanges = false
        });

        // Act & Assert
        validator.IsAllowedAddress(IPAddress.Loopback).ShouldBeFalse();
        validator.IsAllowedAddress(IPAddress.Parse("192.168.1.1")).ShouldBeFalse();
        validator.IsAllowedAddress(IPAddress.Parse("8.8.8.8")).ShouldBeFalse();
    }

    private static LanAccessValidator CreateValidator(LanAccessOptions lanAccessOptions)
    {
        var options = Options.Create(new YARPadOptions { LanAccess = lanAccessOptions });
        return new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
    }
}
