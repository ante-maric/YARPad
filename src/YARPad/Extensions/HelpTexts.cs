using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.SessionAffinity;

namespace CodingCell.YARPad;

internal static class HelpTexts
{
    public static string GetLoadBalancingPolicyText(string policy)
    {
        return policy switch
        {
            _ when policy == LoadBalancingPolicies.FirstAlphabetical => "Select the alphabetically first available destination without considering load.",
            _ when policy == LoadBalancingPolicies.Random => "Select a destination randomly.",
            _ when policy == LoadBalancingPolicies.RoundRobin => "Select a destination by cycling through them in order.",
            _ when policy == LoadBalancingPolicies.LeastRequests => "Select the destination with the least assigned requests. This requires examining all destinations.",
            _ when policy == LoadBalancingPolicies.PowerOfTwoChoices => "Selects two random destinations and picks the one with fewer requests, balancing overhead and randomness.",
            _ => string.Empty
        };
    }

    public static string GetSessionAffinityPolicyText(string? policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.Policies.HashCookie => "Stores the affinity key in a hashed cookie (XxHash64). Fast, no Data Protection required.",
            _ when policy == SessionAffinityConstants.Policies.ArrCookie => "Stores the affinity key in a SHA-256 hashed cookie compatible with IIS ARR.",
            _ when policy == SessionAffinityConstants.Policies.Cookie => "Stores the affinity key in an encrypted cookie using ASP.NET Core Data Protection.",
            _ when policy == SessionAffinityConstants.Policies.CustomHeader => "Stores the affinity key in an encrypted header using ASP.NET Core Data Protection.",
            _ => string.Empty
        };
    }

    public static string GetSessionAffinityFailurPolicyText(string? policy)
    {
        return policy switch
        {
            _ when policy == SessionAffinityConstants.FailurePolicies.Redistribute => "Route request to another healthy destination.",
            _ when policy == SessionAffinityConstants.FailurePolicies.Return503Error => "Return HTTP 503 error.",
            _ => string.Empty
        };
    }

    public static string GetPassiveHealthCheckPolicyText(string? policy)
    {
        return policy switch
        {
            _ when policy == HealthCheckConstants.PassivePolicy.TransportFailureRate => "Marks a destination unhealthy when the percentage of failed requests within a configured time window exceeds the specified limit.",
            _ => string.Empty
        };
    }
    public static string GetActiveHealthCheckPolicyText(string? policy)
    {
        return policy switch
        {
            _ when policy == HealthCheckConstants.ActivePolicy.ConsecutiveFailures => "Marks a destination unhealthy after N consecutive probe failures.",
            _ => string.Empty
        };
    }
    public static string GetAvailableDestinationsPolicyText(string? policy)
    {
        return policy switch
        {
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyAndUnknown => "Only destinations with health 'Healthy' or 'Unknown' are considered available; otherwise return 503.",
            _ when policy == HealthCheckConstants.AvailableDestinations.HealthyOrPanic => "Use HealthyAndUnknown policy; if none are available, mark all destinations available (panic mode).",
            _ => string.Empty
        };
    }
    public static string? GetRateLimitingPolicyText(string? policy)
    {
        return policy switch
        {
            RateLimitingConstants.Disable => "Rate limiter middleware will not apply any policies to this route, even the default policy.",
            RateLimitingConstants.Default => "No rate limiting is performed on requests, but if Rate Limiting middleware is used it will apply default limiter to all routes.",
            _ => null
        };
    }
    public static string? GetCorsPolicyText(string? policy)
    {
        return policy switch
        {
            CorsConstants.Disable => "CORS middleware will refuse the CORS requests.",
            CorsConstants.Default => "Route will use the policy defined in CorsOptions.DefaultPolicy.",
            _ => null
        };
    }
    public static string? GetTimeoutPolicyText(string? policy)
    {
        return policy switch
        {
            TimeoutPolicyConstants.Disable => "Request timeout middleware will not apply timeouts to this route.",
            _ => null
        };
    }

    public static string? GetAuthorizationPolicyText(string? policy)
    {
        return policy switch
        {
            AuthorizationConstants.Default => "Route will use the policy defined in AuthorizationOptions.DefaultPolicy. That policy is pre-configured to require authenticated users.",
            AuthorizationConstants.Anonymous => "Route will not require authorization regardless of any other configuration in the application such as the FallbackPolicy.",
            AuthorizationConstants.LanOnly => "Route will allow only requests coming from the local network (LAN). This policy is provided by YARPad.",
            _ => null
        };
    }

    extension(YARPadConfigurationStatus status)
    {
        public string ToHelpText()
        {
            return status switch
            {
                YARPadConfigurationStatus.Loading => "Loading configuration...",
                YARPadConfigurationStatus.Invalid => "Yarp validation failed, empty configuration applied.",
                YARPadConfigurationStatus.RevertedToPrevious => "Yarp validation failed, using previous valid state.",
                YARPadConfigurationStatus.Applied => "The configuration has been successfully applied.",
                _ => string.Empty
            };
        }
    }

    extension(CookieSecurePolicy policy)
    {
        public string ToHelpText()
        {
            return policy switch
            {
                CookieSecurePolicy.SameAsRequest => "Uses HTTPS for cookies if the request is HTTPS, otherwise allows both HTTP and HTTPS (ideal for local and mixed environments).",
                CookieSecurePolicy.Always => "Always marks cookies as secure, requiring HTTPS for all authenticated and development environments.",
                CookieSecurePolicy.None => "Never marks cookies as secure, allowing HTTP but risking exposure of authentication data.",
                _ => string.Empty
            };
        }
    }

    extension(SameSiteMode mode)
    {
        public string ToHelpText()
        {
            return mode switch
            {
                SameSiteMode.Unspecified => "No SameSite field will be set, the client should follow its default cookie policy.",
                SameSiteMode.None => "Indicates the client should disable same-site restrictions.",
                SameSiteMode.Lax => "Indicates the client should send the cookie with \"same-site\" requests, and with \"cross-site\" top-level navigations.",
                SameSiteMode.Strict => "Indicates the client should only send the cookie with \"same-site\" requests.",
                _ => string.Empty
            };
        }
    }

    extension(QueryParameterMatchMode mode)
    {
        public string ToHelpText()
        {
            return mode switch
            {
                QueryParameterMatchMode.Exact => "Query string must match in its entirety. Only a single query parameter with the same name is supported.",
                QueryParameterMatchMode.Contains => "Query string key must be present and its value must contain the specified substring. Only a single query parameter with the same name is supported.",
                QueryParameterMatchMode.NotContains => "Query string key must be present and its value must not contain any of the specified substrings. Only a single query parameter with the same name is supported.",
                QueryParameterMatchMode.Prefix => "Query string key must be present and its value must start with the specified prefix. Only a single query parameter with the same name is supported.",
                QueryParameterMatchMode.Exists => "Query string key must exist and have a non-empty value.",
                _ => string.Empty
            };
        }
    }

    extension(HeaderMatchMode mode)
    {
        public string ToHelpText()
        {
            return mode switch
            {
                HeaderMatchMode.ExactHeader => "The header must match exactly, including case sensitivity.",
                HeaderMatchMode.HeaderPrefix => "The header must start with the specified prefix, including case sensitivity.",
                HeaderMatchMode.Contains => "The header must contain the specified value.",
                HeaderMatchMode.NotContains => "The header must not contain the specified value.",
                HeaderMatchMode.Exists => "The header must exist and have a non-empty value.",
                HeaderMatchMode.NotExists => "The header must not exist.",
                _ => string.Empty
            };
        }
    }

    extension(ForwardedTransformNode node)
    {
        public string ToHelpText()
        {
            return node switch
            {
                ForwardedTransformNode.For => "Client IP.",
                ForwardedTransformNode.By => "Proxy info.",
                ForwardedTransformNode.Proto => "Original protocol (http/https).",
                ForwardedTransformNode.Host => "Original host name.",
                _ => string.Empty
            };
        }
    }

    extension(ForwardedTransformAction action)
    {
        public string ToHelpText()
        {
            return action switch
            {
                ForwardedTransformAction.Set => "Overwrite existing header.",
                ForwardedTransformAction.Append => "Add to existing header.",
                ForwardedTransformAction.Remove => "Delete header.",
                ForwardedTransformAction.Off => "Disable this transform.",
                _ => string.Empty
            };
        }
    }
}