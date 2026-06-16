using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.SinglePageCheckout.Infrastructure;

/// <summary>
/// A view location expander that swaps nopCommerce's core <c>Checkout/OnePageCheckout</c>
/// view for the single-page checkout view shipped by this plugin. It is gated on the
/// plugin's <see cref="SinglePageCheckoutSettings.Enabled"/> flag, so disabling the plugin
/// (or the setting) restores the default checkout without any route changes.
/// </summary>
public class SinglePageCheckoutViewLocationExpander : IViewLocationExpander
{
    protected const string ENABLED_KEY = "nop.plugin.misc.singlepagecheckout.enabled";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        //the admin area is never overridden
        if (context.AreaName?.Equals(AreaNames.ADMIN) ?? false)
            return;

        //only relevant for the core one-page checkout view
        if (!IsCheckoutView(context))
            return;

        var enabled = false;
        try
        {
            var settings = EngineContext.Current.Resolve<SinglePageCheckoutSettings>();
            enabled = settings?.Enabled ?? false;
        }
        catch
        {
            //settings not available yet (e.g. before installation) - leave the default view in place
        }

        //the value participates in the view-location cache key so toggling the setting re-resolves the view
        context.Values[ENABLED_KEY] = enabled ? "1" : "0";
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (IsCheckoutView(context) &&
            context.Values.TryGetValue(ENABLED_KEY, out var enabled) &&
            enabled == "1")
        {
            return new[] { MiscSinglePageCheckoutDefaults.OverrideViewLocation }.Concat(viewLocations);
        }

        return viewLocations;
    }

    protected static bool IsCheckoutView(ViewLocationExpanderContext context)
    {
        return string.Equals(context.ViewName, MiscSinglePageCheckoutDefaults.OverriddenViewName, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(context.ControllerName, MiscSinglePageCheckoutDefaults.OverriddenControllerName, StringComparison.OrdinalIgnoreCase);
    }
}
