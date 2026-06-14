using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.OrderItemsRowDetails;

/// <summary>
/// Represents constants for the Order items row details plugin
/// </summary>
public static class WidgetsOrderItemsRowDetailsDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Widgets.OrderItemsRowDetails";

    /// <summary>
    /// Gets the admin widget zone on the order list page where the behavior is injected
    /// </summary>
    public static string WidgetZone => AdminWidgetZones.OrderListButtons;

    /// <summary>
    /// Gets the DOM id of the admin order list DataTables grid that is enhanced
    /// </summary>
    public static string OrdersGridId => "orders-grid";
}
