namespace CodingCell.YARPad;

public sealed record SessionAffinityModel
{
    public string? Policy { get; set; }

    public string? FailurePolicy { get; set; }

    public required string AffinityKeyName { get; set; }

    public SessionAffinityCookieModel Cookie { get; set; } = new();
}
