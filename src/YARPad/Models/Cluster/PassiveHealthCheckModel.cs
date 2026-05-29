namespace CodingCell.YARPad;

public sealed record PassiveHealthCheckModel
{
    public bool? Enabled { get; set; }

    public string? Policy { get; set; }

    public TimeSpan? ReactivationPeriod { get; set; }
}
