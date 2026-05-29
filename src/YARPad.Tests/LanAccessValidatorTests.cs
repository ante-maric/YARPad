using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace CodingCell.YARPad.Tests;

public class LanAccessValidatorTests
{
    [Theory]
    [InlineData("127.0.0.1", true)]  // Loopback IPv4
    [InlineData("::1", true)]  // Loopback IPv6
    [InlineData("10.0.0.1", true)]  // Private Class A
    [InlineData("172.16.0.1", true)]  // Private Class B
    [InlineData("192.168.1.1", true)]  // Private Class C
    [InlineData("8.8.8.8", false)]  // Public IP (Google DNS)
    [InlineData("1.1.1.1", false)]  // Public IP (Cloudflare DNS)
    public void IsAllowedAddress_WithDefaultSettings_ValidatesCorrectly(string ipString, bool expectedResult)
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = true,
                IncludeDefaultPrivateRanges = true
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var ipAddress = IPAddress.Parse(ipString);

        // Act
        var result = validator.IsAllowedAddress(ipAddress);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void IsAllowedAddress_WithLoopbackDisabled_BlocksLoopback()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = false,
                IncludeDefaultPrivateRanges = true
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var loopback = IPAddress.Loopback;

        // Act
        var result = validator.IsAllowedAddress(loopback);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAllowedAddress_WithAdditionalAllowedRanges_AllowsCustomRange()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = false,
                IncludeDefaultPrivateRanges = false,
                AdditionalAllowedRanges = ["203.0.113.0/24"]
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var customIp = IPAddress.Parse("203.0.113.100");
        var outsideIp = IPAddress.Parse("203.0.114.100");

        // Act
        var allowedResult = validator.IsAllowedAddress(customIp);
        var blockedResult = validator.IsAllowedAddress(outsideIp);

        // Assert
        allowedResult.ShouldBeTrue();
        blockedResult.ShouldBeFalse();
    }

    [Fact]
    public void IsAllowedAddress_WithIPv6ULA_AllowsUniqueLocalAddresses()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = true,
                IncludeDefaultPrivateRanges = true
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var ulaAddress = IPAddress.Parse("fc00::1");

        // Act
        var result = validator.IsAllowedAddress(ulaAddress);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedAddress_WithIPv6LinkLocal_AllowsLinkLocalAddresses()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = true,
                IncludeDefaultPrivateRanges = true
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var linkLocalAddress = IPAddress.Parse("fe80::1");

        // Act
        var result = validator.IsAllowedAddress(linkLocalAddress);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedAddress_WithIPv4MappedToIPv6_ValidatesAsIPv4()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = true,
                IncludeDefaultPrivateRanges = true
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        // 192.168.1.1 mapped to IPv6
        var mappedAddress = IPAddress.Parse("::ffff:192.168.1.1");

        // Act
        var result = validator.IsAllowedAddress(mappedAddress);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedAddress_WithDefaultPrivateRangesDisabled_BlocksPrivateIPs()
    {
        // Arrange
        var options = Options.Create(new YARPadOptions
        {
            LanAccess = new LanAccessOptions
            {
                AllowLoopback = false,
                IncludeDefaultPrivateRanges = false
            }
        });
        var validator = new LanAccessValidator(options, NullLogger<LanAccessValidator>.Instance);
        var privateIp = IPAddress.Parse("192.168.1.1");

        // Act
        var result = validator.IsAllowedAddress(privateIp);

        // Assert
        result.ShouldBeFalse();
    }
}
