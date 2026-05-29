namespace CodingCell.YARPad;

public sealed record RouteMatchModel
{
    public string? Path { get; set; }

    public List<string> Hosts { get; set; } = [];

    public List<string> Methods { get; set; } = [];

    public List<RouteHeaderModel> Headers { get; set; } = [];

    public List<RouteQueryParameterModel> QueryParameters { get; set; } = [];
}
