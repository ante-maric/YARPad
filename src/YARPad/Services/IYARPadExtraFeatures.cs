using Microsoft.AspNetCore.Components;

namespace CodingCell.YARPad;

public interface IYARPadExtraFeatures
{
    bool HasExtraFeatures { get; }

    void ViewLogs(string routeID);

    RenderFragment? GetRouteExtraActions(RouteModel route);
}
