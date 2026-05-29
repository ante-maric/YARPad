namespace CodingCell.YARPad;

public sealed record HealthCheckModel
{
    public PassiveHealthCheckModel Passive { get; set; } = new();

    public ActiveHealthCheckModel Active { get; set; } = new();

    public string? AvailableDestinationsPolicy { get; set; }
}
