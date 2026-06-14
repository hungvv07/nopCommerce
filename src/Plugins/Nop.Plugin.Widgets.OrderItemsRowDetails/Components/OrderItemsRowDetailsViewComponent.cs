using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.OrderItemsRowDetails.Components;

/// <summary>
/// Renders the script that wires the row-details behavior into the admin order list grid
/// </summary>
public class OrderItemsRowDetailsViewComponent : NopViewComponent
{
    /// <returns>A task that represents the asynchronous operation</returns>
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        //only enhance the order list page button zone
        if (!string.Equals(widgetZone, WidgetsOrderItemsRowDetailsDefaults.WidgetZone, StringComparison.OrdinalIgnoreCase))
            return Content(string.Empty);

        return View("~/Plugins/Widgets.OrderItemsRowDetails/Views/PublicInfo.cshtml");
    }
}
