namespace Nop.Plugin.Misc.SinglePageCheckout;

/// <summary>
/// Represents constants for the Single Page Checkout plugin
/// </summary>
public static class MiscSinglePageCheckoutDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Misc.SinglePageCheckout";

    /// <summary>
    /// Gets the admin configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Misc.SinglePageCheckout.Configure";

    /// <summary>
    /// Gets the view name of the core one-page checkout action whose view this plugin overrides
    /// </summary>
    public static string OverriddenViewName => "OnePageCheckout";

    /// <summary>
    /// Gets the name of the core controller whose checkout view this plugin overrides
    /// </summary>
    public static string OverriddenControllerName => "Checkout";

    /// <summary>
    /// Gets the physical path of the single-page checkout view shipped by this plugin
    /// </summary>
    public static string OverrideViewLocation => "~/Plugins/Misc.SinglePageCheckout/Views/OnePageCheckout.cshtml";
}
