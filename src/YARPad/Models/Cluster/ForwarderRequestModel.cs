namespace CodingCell.YARPad;

public sealed record ForwarderRequestModel
{
    public TimeSpan? ActivityTimeout { get; set; }

    public Version? Version { get; set; }

    public HttpVersionPolicy? VersionPolicy { get; set; }

    public bool? AllowResponseBuffering { get; set; }
}
