namespace CodingCell.YARPad;

public enum PolicyType
{
    //Route
    Authorization,
    Timeout,
    RateLimiter,
    Cors,
    OutputCache,

    //Cluster
    LoadBalancing, //don't move this (PolicyType.GetContext)
    SessionAffinity,
    SessionAffinityFailure,
    ActiveHealthCheck,
    PassiveHealthCheck,
    AvailableDestination
}
