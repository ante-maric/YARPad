using Microsoft.AspNetCore.Http;
using MudBlazor;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public static class ColorResolver
{
    public static class Buttons
    {
        public static Color Add { get; } = Color.Primary;
        public static Color AddSubtle { get; } = Color.Primary;
        public static Color Edit { get; } = Color.Success;
        public static Color Delete { get; } = Color.Error;
        public static Color Save { get; } = Color.Primary;
        public static Color Switch { get; } = Color.Primary;
        public static Color Toggle { get; } = Color.Primary;
        public static Color LinkSubtle { get; } = Color.Default;
    }

    public static class Symbol
    {
        public static Color Clusters { get; } = Color.Secondary;
        public static Color Locked { get; } = Color.Warning;
        public static string Label { get; } = YarpadColor.Purple;
        public static Color Timeout { get; } = Color.Warning;
        public static Color BadgeError { get; } = Color.Error;
        public static Color Theme { get; } = Color.Primary;
        public static Color ToggleOff { get; } = Color.Error;
        public static Color ToggleOn { get; } = Color.Success;
        public static Color HeaderValue { get; } = Color.Secondary;
        public static Color Host { get; } = Color.Primary;
    }

    public static string GetPolicySectionIconColorClass(int policiesCount)
    {
        return policiesCount > 0
            ? RouteConfigSection.General.ToIconColorClass()
            : YarpadColor.Gray;
    }

    public static string GetPolicySectionChipColorClasses(int policiesCount)
    {
        return policiesCount > 0
            ? RouteConfigSection.General.ToChipColorClasses()
            : YarpadColor.Gray;
    }

    public static string GetLoadBalancingPolicyColorClass(string policy)
    {
        return policy switch
        {
            _ when policy == LoadBalancingPolicies.PowerOfTwoChoices => YarpadColor.Primary,
            _ when policy == LoadBalancingPolicies.Random => YarpadColor.Yellow,
            _ when policy == LoadBalancingPolicies.LeastRequests => YarpadColor.Green,
            _ when policy == LoadBalancingPolicies.FirstAlphabetical => YarpadColor.Indigo,
            _ when policy == LoadBalancingPolicies.RoundRobin => YarpadColor.Cyan,
            _ => YarpadColor.Gray
        };
    }

    public static string GetSessionAffinityPolicyColorClass(string policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.Policies.Cookie => YarpadColor.Orange,
            _ when policy == SessionAffinityConstants.Policies.HashCookie => YarpadColor.Blue,
            _ when policy == SessionAffinityConstants.Policies.ArrCookie => YarpadColor.Purple,
            _ when policy == SessionAffinityConstants.Policies.CustomHeader => YarpadColor.Green,
            _ => YarpadColor.Gray
        };
    }

    public static string GetSessionAffinityFailurePolicyColorClass(string policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.FailurePolicies.Redistribute => YarpadColor.Green,
            _ when policy == SessionAffinityConstants.FailurePolicies.Return503Error => YarpadColor.Red,
            _ => YarpadColor.Gray
        };
    }

    public static string GetAvailableDestinationsPolicyColorClass(string? policy)
    {
        return policy switch
        {
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyAndUnknown => YarpadColor.Green,
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyOrPanic => YarpadColor.Orange,
            _ => YarpadColor.Gray
        };
    }

    public static Color GetWebProxyColor(bool isEnabled)
    {
        return isEnabled
            ? Color.Success
            : Color.Default;
    }

    public static Color GetSslValidationColor(bool isEnabled) => isEnabled == true ? Color.Success : Color.Error;

    public static string GetHttpMethodChipClasses(string httpMethod)
    {
        return $"yarpad-chip-color-custom yarpad-chip-color-{httpMethod.ToLower()}";
    }

    public static Color GetSwitchThumbIconColor(bool? isEnabled) => isEnabled == true ? Color.Success : Color.Error;

    public static Color GetConfigActivationColor(bool isActive) => isActive ? Color.Secondary : Color.Default;

    extension(HttpVersionPolicy? policy)
    {
        public string ToColorClass()
        {
            return policy switch
            {
                HttpVersionPolicy.RequestVersionOrLower => YarpadColor.Orange,
                HttpVersionPolicy.RequestVersionOrHigher => YarpadColor.Teal,
                HttpVersionPolicy.RequestVersionExact => YarpadColor.Indigo,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(HeaderMatchMode mode)
    { 
        public string ToColorClass()
        {
            return mode switch
            {
                HeaderMatchMode.ExactHeader => YarpadColor.Purple,
                HeaderMatchMode.HeaderPrefix => YarpadColor.Indigo,
                HeaderMatchMode.Contains => YarpadColor.Lime,
                HeaderMatchMode.NotContains => YarpadColor.Orange,
                HeaderMatchMode.Exists => YarpadColor.Green,
                HeaderMatchMode.NotExists => YarpadColor.Red,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(QueryParameterMatchMode mode)
    {
        public string ToColorClass()
        {
            return mode switch
            {
                QueryParameterMatchMode.Exact => YarpadColor.Purple,
                QueryParameterMatchMode.Contains => YarpadColor.Lime,
                QueryParameterMatchMode.NotContains => YarpadColor.Orange,
                QueryParameterMatchMode.Prefix => YarpadColor.Indigo,
                QueryParameterMatchMode.Exists => YarpadColor.Green,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(ClusterConfigSection section)
    {
        public string ToChipColorClasses()
        {
            return "yarpad-chip-color-custom yarpad-chip-color-" + section switch
            {
                ClusterConfigSection.SessionAffinity => nameof(YarpadColor.Purple).ToLower(),
                ClusterConfigSection.HealthCheck => nameof(YarpadColor.Green).ToLower(),
                ClusterConfigSection.HttpClient => nameof(YarpadColor.Cyan).ToLower(),
                ClusterConfigSection.HttpRequest => nameof(YarpadColor.Orange).ToLower(),
                ClusterConfigSection.Metadata => nameof(YarpadColor.Yellow).ToLower(),
                _ => nameof(YarpadColor.Gray).ToLower()
            };
        }

        public string ToIconColorClass()
        {
            return section switch
            {
                ClusterConfigSection.SessionAffinity => YarpadColor.Purple,
                ClusterConfigSection.HealthCheck => YarpadColor.Green,
                ClusterConfigSection.HttpClient => YarpadColor.Cyan,
                ClusterConfigSection.HttpRequest => YarpadColor.Orange,
                ClusterConfigSection.Metadata => YarpadColor.Yellow,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(RouteConfigSection section)
    {
        public string ToChipColorClasses()
        {
            return "yarpad-chip-color-custom yarpad-chip-color-" + section switch
            {
                RouteConfigSection.General => nameof(YarpadColor.Orange).ToLower(),
                RouteConfigSection.Match => nameof(YarpadColor.Blue).ToLower(),
                RouteConfigSection.Transform => nameof(YarpadColor.Purple).ToLower(),
                RouteConfigSection.Metadata => nameof(YarpadColor.Yellow).ToLower(),
                _ => nameof(YarpadColor.Gray).ToLower()
            };
        }

        public string ToIconColorClass()
        {
            return section switch
            {
                RouteConfigSection.General => YarpadColor.Orange,
                RouteConfigSection.Match => YarpadColor.Blue,
                RouteConfigSection.Transform => YarpadColor.Purple,
                RouteConfigSection.Metadata => YarpadColor.Yellow,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(CookieSecurePolicy section)
    {
        public string ToColorClass()
        {
            return section switch
            {
                CookieSecurePolicy.SameAsRequest => YarpadColor.Blue,
                CookieSecurePolicy.Always => YarpadColor.Green,
                CookieSecurePolicy.None => YarpadColor.Red,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(SameSiteMode section)
    {
        public string ToColorClass()
        {
            return section switch
            {
                SameSiteMode.Unspecified => YarpadColor.Gray,
                SameSiteMode.None => YarpadColor.Red,
                SameSiteMode.Lax => YarpadColor.Yellow,
                SameSiteMode.Strict => YarpadColor.Green,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(bool? value)
    {
        public Color ToColor()
        {
            return value switch
            {
                true => Color.Success,
                _ => Color.Default
            };
        }
    }

    extension(RouteTransformGroup group)
    {
        public string ToColorClass()
        {
            return group switch
            {
                RouteTransformGroup.Path => YarpadColor.Red,
                RouteTransformGroup.Query => YarpadColor.Yellow,
                RouteTransformGroup.RequestHeaders => YarpadColor.Blue,
                RouteTransformGroup.ResponseHeaders => YarpadColor.Cyan,
                RouteTransformGroup.ResponseTrailers => YarpadColor.Green,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(PolicyType policyType)
    {
        public string ToColorClass()
        {
            return policyType switch
            {
                PolicyType.Authorization => YarpadColor.Blue,
                PolicyType.Timeout => YarpadColor.Orange,
                PolicyType.RateLimiter => YarpadColor.Red,
                PolicyType.Cors => YarpadColor.Cyan,
                PolicyType.OutputCache => YarpadColor.Lime,
                PolicyType.LoadBalancing => YarpadColor.Blue,
                PolicyType.SessionAffinity => YarpadColor.Teal,
                PolicyType.SessionAffinityFailure => YarpadColor.Red,
                PolicyType.ActiveHealthCheck => YarpadColor.Green,
                PolicyType.PassiveHealthCheck => YarpadColor.Gray,
                PolicyType.AvailableDestination => YarpadColor.Green,
                _ => YarpadColor.Gray
            };
        }
    }

    extension(YARPadConfigurationStatus status)
    {
        public Color ToColor()
        {
            return status switch
            {
                YARPadConfigurationStatus.Loading => Color.Warning,
                YARPadConfigurationStatus.Invalid => Color.Error,
                YARPadConfigurationStatus.Applied => Color.Success,
                YARPadConfigurationStatus.RevertedToPrevious => Color.Warning,
                _ => Color.Default
            };
        }
    }
}
