using Microsoft.AspNetCore.Components;

namespace CodingCell.YARPad;

public class YARPadNoExtraFeatures : IYARPadExtraFeatures
{
    public bool HasExtraFeatures => false;

    public void ViewLogs(string routeID)
    {
    }

    public RenderFragment? GetRouteExtraActions(RouteModel route) => null;
}
