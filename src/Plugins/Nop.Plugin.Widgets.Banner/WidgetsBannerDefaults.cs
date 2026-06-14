using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.Banner;

/// <summary>
/// Represents constants for the Banner widget plugin
/// </summary>
public static class WidgetsBannerDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Widgets.Banner";

    /// <summary>
    /// Gets the widget zone used when none has been configured yet
    /// </summary>
    public static string DefaultWidgetZone => PublicWidgetZones.HomepageTop;

    /// <summary>
    /// Gets the public widget zones an admin can choose to display the banner in
    /// </summary>
    public static IReadOnlyList<string> SelectableWidgetZones => new[]
    {
        PublicWidgetZones.HomepageTop,
        PublicWidgetZones.HomepageBottom,
        PublicWidgetZones.HeaderBefore,
        PublicWidgetZones.HeaderAfter,
        PublicWidgetZones.HeaderMenuBefore,
        PublicWidgetZones.HeaderMenuAfter,
        PublicWidgetZones.ContentBefore,
        PublicWidgetZones.ContentAfter,
        PublicWidgetZones.MainColumnBefore,
        PublicWidgetZones.MainColumnAfter,
        PublicWidgetZones.Footer
    };
}
