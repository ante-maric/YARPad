using Microsoft.AspNetCore.Http;
using MudBlazor;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;
using Yarp.ReverseProxy.Configuration;

namespace CodingCell.YARPad;

public static class IconResolver
{
    private const string CUSTOM = Icons.Material.Filled.DesignServices;

    public static class Buttons
    {
        public static string AddSubtle { get; } = Icons.Material.Filled.Add;
        public static string Add { get; } = Icons.Material.Filled.AddCircle;
        public static string Edit { get; } = Icons.Material.Filled.Edit;
        public static string Delete { get; } = Icons.Material.Filled.Delete;
        public static string More { get; } = Icons.Material.Filled.MoreVert;
        public static string Clone { get; } = Icons.Material.Filled.ContentCopy;
        public static string Menu { get; } = Icons.Material.Filled.Menu;
        public static string MoveUp { get; } = Icons.Material.Filled.ArrowUpward;
        public static string MoveDown { get; } = Icons.Material.Filled.ArrowDownward;
        public static string Import { get; } = "fa-solid fa-file-import";
        public static string Export { get; } = "fa-solid fa-file-export";
        public static string Json { get; } = Icons.Material.Filled.DataObject;
        public static string Close { get; } = Icons.Material.Filled.Close;
    }

    public static class Thumb
    {
        public static string On { get; } = "Icons.Material.Filled.Done";
        public static string Off { get; } = Icons.Material.Filled.Close;
        public static string Dark { get; } = Icons.Material.Filled.Bedtime;
        public static string Light { get; } = Icons.Material.Filled.WbSunny;
    }

    public static class Symbol
    {
        public static string Timeout { get; } = Icons.Material.Filled.HourglassBottom;
        public static string Label { get; } = Icons.Material.Filled.Label;
        public static string Route { get; } = Icons.Material.Filled.AltRoute;
        public static string Cluster { get; } = Icons.Material.Filled.Hub;
        public static string Configuration { get; } = Icons.Material.Filled.Settings;
        public static string CustomTransform { get; } = Icons.Material.Filled.Tune;
        public static string Policy { get; } = Icons.Material.Filled.Policy;
        public static string Locked { get; } = Icons.Material.Filled.Lock;
        public static string ToggleOn { get; } = Icons.Material.Filled.ToggleOn;
        public static string ToggleOff { get; } = Icons.Material.Filled.ToggleOff;
        public static string Unknown { get; } = Icons.Material.Filled.QuestionMark;
        public static string SessionAffinity { get; } = "fa-solid fa-link";
        public static string ActiveConfig { get; } = "fa-solid fa-play";
        public static string InactiveConfig { get; } = "fa-solid fa-pause";

        public static string MatchModeExact { get; } = Icons.Material.Filled.Spellcheck;
        public static string MatchModeContains { get; } = Icons.Material.Filled.Search;
        public static string MatchModeNotContains { get; } = Icons.Material.Filled.SearchOff;
        public static string MatchModePrefix { get; } = Icons.Material.Filled.Segment;
        public static string MatchModeExists { get; } = Icons.Material.Filled.CheckCircleOutline;
        public static string MatchModeNotExists { get; } = Icons.Material.Filled.HighlightOff;

        public static string User { get; } = "fa-solid fa-circle-user";
        public static string Logout { get; } = "fa-solid fa-right-from-bracket";
    }

    public static string GetLoadBalancingPolicyIcon(string policy)
    {
        return policy switch
        {
            _ when policy == LoadBalancingPolicies.PowerOfTwoChoices => Icons.Material.Filled.Balance,
            _ when policy == LoadBalancingPolicies.Random => Icons.Material.Filled.Casino,
            _ when policy == LoadBalancingPolicies.LeastRequests => Icons.Material.Filled.TrendingDown,
            _ when policy == LoadBalancingPolicies.FirstAlphabetical => Icons.Material.Filled.SortByAlpha,
            _ when policy == LoadBalancingPolicies.RoundRobin => Icons.Material.Filled.Loop,
            _ => CUSTOM
        };
    }

    public static string GetSessionAffinityPolicyIcon(string policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.Policies.Cookie => Icons.Material.Filled.Cookie,
            _ when policy == SessionAffinityConstants.Policies.HashCookie => Icons.Material.Filled.Tag,
            _ when policy == SessionAffinityConstants.Policies.ArrCookie => Icons.Material.Filled.ForkLeft,
            _ when policy == SessionAffinityConstants.Policies.CustomHeader => Icons.Material.Filled.HMobiledata,
            _ => CUSTOM
        };
    }

    public static string GetSessionAffinityFailurePolicyIcon(string policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.FailurePolicies.Redistribute => Icons.Material.Filled.CallSplit,
            _ when policy == SessionAffinityConstants.FailurePolicies.Return503Error => Icons.Material.Outlined.ErrorOutline,
            _ => CUSTOM
        };
    }

    public static string GetAvailableDestinationsPolicyIcon(string? policy)
    {
        return policy switch
        {
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyAndUnknown => Icons.Material.Filled.MonitorHeart,
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyOrPanic => "fa-solid fa-bolt",
            _ when policy == null => "fa-solid fa-bolt",
            _ => CUSTOM
        };
    }

    public static string GetWebProxyIcon(bool isEnabled)
    {
        return isEnabled
            ? Icons.Material.Filled.Public
            : Icons.Material.Filled.PublicOff;
    }

    public static string GetSslValidationIcon(bool isEnabled)
    {
        return isEnabled
            ? "fa-solid fa-lock"
            : "fa-solid fa-lock-open";
    }

    public static string GetSwitchThumbIcon(bool? isEnabled) =>
        isEnabled == true ? Thumb.On : Thumb.Off;

    public static string GetThemeIcon(bool isDark) =>
        isDark ? Thumb.Dark : Thumb.Light;

    public static string GetConfigActivationIcon(bool isActive) =>
        isActive ? Symbol.ActiveConfig : Symbol.InactiveConfig;

    extension(HttpVersionPolicy? policy)
    {
        public string ToIcon()
        {
            return policy switch
            {
                HttpVersionPolicy.RequestVersionOrLower => Icons.Material.Filled.ArrowDownward,
                HttpVersionPolicy.RequestVersionOrHigher => Icons.Material.Filled.ArrowUpward,
                HttpVersionPolicy.RequestVersionExact => Icons.Material.Filled.CompareArrows,
                _ => CUSTOM
            };
        }
    }

    extension(HeaderMatchMode mode)
    {
        public string ToIcon()
        {
            return mode switch
            {
                HeaderMatchMode.ExactHeader => Symbol.MatchModeExact,
                HeaderMatchMode.HeaderPrefix => Symbol.MatchModePrefix,
                HeaderMatchMode.Contains => Symbol.MatchModeContains,
                HeaderMatchMode.NotContains => Symbol.MatchModeNotContains,
                HeaderMatchMode.Exists => Symbol.MatchModeExists,
                HeaderMatchMode.NotExists => Symbol.MatchModeNotExists,
                _ => CUSTOM
            };
        }
    }

    extension(QueryParameterMatchMode mode)
    {
        public string ToIcon()
        {
            return mode switch
            {
                QueryParameterMatchMode.Exact => Symbol.MatchModeExact,
                QueryParameterMatchMode.Contains => Symbol.MatchModeContains,
                QueryParameterMatchMode.NotContains => Symbol.MatchModeNotContains,
                QueryParameterMatchMode.Prefix => Symbol.MatchModePrefix,
                QueryParameterMatchMode.Exists => Symbol.MatchModeExists,
                _ => CUSTOM
            };
        }
    }

    extension(bool? value)
    {
        public string ToIcon()
        {
            return value switch
            {
                true => Symbol.ToggleOn,
                _ => Symbol.ToggleOff
            };
        }
    }

    extension(ClusterConfigSection section)
    {
        public string ToIcon()
        {
            return section switch
            {
                ClusterConfigSection.Destinations => "fa-solid fa-arrows-to-dot",
                ClusterConfigSection.SessionAffinity => Symbol.SessionAffinity,
                ClusterConfigSection.HealthCheck => "fa-solid fa-heart-pulse",
                ClusterConfigSection.HttpClient => Icons.Material.Filled.SettingsEthernet,
                ClusterConfigSection.HttpRequest => Icons.Material.Filled.Send,
                ClusterConfigSection.Metadata => CUSTOM,
                _ => CUSTOM
            };
        }
    }

    extension(RouteConfigSection section)
    {
        public string ToIcon()
        {
            return section switch
            {
                RouteConfigSection.Match => Icons.Material.Filled.CallSplit,
                RouteConfigSection.Transform => Icons.Material.Filled.AutoFixHigh,
                RouteConfigSection.Metadata => Icons.Material.Filled.Info,
                _ => CUSTOM
            };
        }
    }

    extension(CookieSecurePolicy section)
    {
        public string ToIcon()
        {
            return section switch
            {
                CookieSecurePolicy.SameAsRequest => "fa-solid fa-link",
                CookieSecurePolicy.Always => "fa-solid fa-lock",
                CookieSecurePolicy.None => "fa-solid fa-lock-open",
                _ => Symbol.Unknown
            };
        }
    }

    extension(SameSiteMode section)
    {
        public string ToIcon()
        {
            return section switch
            {
                SameSiteMode.Unspecified => Symbol.Unknown,
                SameSiteMode.None => Icons.Material.Filled.Language,
                SameSiteMode.Lax => Icons.Material.Filled.CompareArrows,
                SameSiteMode.Strict => "fa-solid fa-key",
                _ => Symbol.Unknown
            };
        }
    }

    extension(RouteTransformGroup group)
    {
        public string ToIcon()
        {
            return group switch
            {
                RouteTransformGroup.Path => Icons.Material.Filled.Link,
                RouteTransformGroup.Query => Symbol.Unknown,
                RouteTransformGroup.RequestHeaders => Icons.Material.Filled.Send,
                RouteTransformGroup.ResponseHeaders => Icons.Material.Filled.Reply,
                RouteTransformGroup.ResponseTrailers => Icons.Material.Filled.ReplyAll,
                _ => Icons.Material.Filled.Category
            };
        }
    }

    extension(PolicyType policyType)
    {
        public string ToIcon()
        {
            return policyType switch
            {
                PolicyType.Authorization => Icons.Material.Filled.Security,
                PolicyType.Timeout => Icons.Material.Filled.Timer,
                PolicyType.RateLimiter => Icons.Material.Filled.Speed,
                PolicyType.Cors => Icons.Material.Filled.Public,
                PolicyType.OutputCache => Icons.Material.Filled.Memory,
                PolicyType.LoadBalancing => "fa-solid fa-arrows-up-to-line",
                PolicyType.SessionAffinity => Symbol.SessionAffinity,
                PolicyType.SessionAffinityFailure => "fa-solid fa-link-slash",
                PolicyType.ActiveHealthCheck => Icons.Material.Filled.MonitorHeart,
                PolicyType.PassiveHealthCheck => Icons.Material.Filled.HealthAndSafety,
                PolicyType.AvailableDestination => Icons.Material.Filled.LocationOn,
                _ => CUSTOM
            };
        }
    }

    extension(PolicyContext context)
    {
        public string ToIcon()
        {
            return context switch
            {
                PolicyContext.Route => Symbol.Route,
                PolicyContext.Cluster => Symbol.Cluster,
                _ => string.Empty
            };
        }
    }

    extension(YarpConfigurationSection context)
    {
        public string ToIcon()
        {
            return context switch
            {
                YarpConfigurationSection.Route => Symbol.Route,
                YarpConfigurationSection.Cluster => Symbol.Cluster,
                _ => string.Empty
            };
        }
    }

    extension(YARPadConfigurationStatus status)
    {
        public string ToIcon()
        {
            return status switch
            {
                YARPadConfigurationStatus.Loading => Icons.Material.Filled.HourglassEmpty,
                YARPadConfigurationStatus.Invalid => Icons.Material.Filled.Error,
                YARPadConfigurationStatus.Applied => Icons.Material.Filled.CheckCircle,
                YARPadConfigurationStatus.RevertedToPrevious => Icons.Material.Filled.History,
                _ => string.Empty
            };
        }
    }
}

