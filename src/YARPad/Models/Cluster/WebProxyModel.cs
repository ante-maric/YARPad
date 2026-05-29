namespace CodingCell.YARPad;

public sealed record WebProxyModel : IEquatable<WebProxyModel>
{
    public string? Address { get; set; }

    public bool? BypassOnLocal { get; set; }

    public bool? UseDefaultCredentials { get; set; }
}
