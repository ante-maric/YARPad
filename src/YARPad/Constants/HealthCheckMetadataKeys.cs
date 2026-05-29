namespace CodingCell.YARPad;

public static class HealthCheckMetadataKeys
{
    public static class ActiveHealthCheck
    {
        public const string ConsecutiveFailuresThreshold = "ConsecutiveFailuresHealthPolicy.Threshold";
    }

    public static class PassiveHealthCheck
    {
        public const string TransportFailureRateLimit = "TransportFailureRateHealthPolicy.RateLimit";
    }
}
