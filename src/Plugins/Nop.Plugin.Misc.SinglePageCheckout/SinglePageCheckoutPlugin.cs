using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.SinglePageCheckout;

/// <summary>
/// Represents the Single Page Checkout plugin. It replaces nopCommerce's default
/// multi-step / accordion checkout with a flat single-page layout, reusing the
/// existing checkout pipeline (the Opc* controller actions and model factories).
/// </summary>
public class SinglePageCheckoutPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INopUrlHelper _nopUrlHelper;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly OrderSettings _orderSettings;

    #endregion

    #region Ctor

    public SinglePageCheckoutPlugin(ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        IWebHelper webHelper,
        OrderSettings orderSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _webHelper = webHelper;
        _orderSettings = orderSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/MiscSinglePageCheckout/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new SinglePageCheckoutSettings
        {
            Enabled = true,
            LayoutType = CheckoutLayoutType.Layout01,
            ShowDiscountBox = true,
            ShowGiftCardBox = true,
            ShowCheckoutAttributes = true,
            ShowEstimateShipping = true
        });

        //the plugin overrides the one-page checkout view, so make sure the store routes the
        //"Checkout" button to that page rather than the multi-step flow
        if (!_orderSettings.OnePageCheckoutEnabled)
        {
            _orderSettings.OnePageCheckoutEnabled = true;
            await _settingService.SaveSettingAsync(_orderSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.SinglePageCheckout.Fields.Enabled"] = "Enabled",
            ["Plugins.Misc.SinglePageCheckout.Fields.Enabled.Hint"] = "When enabled, the storefront checkout is rendered as a single page. When disabled, nopCommerce falls back to its default checkout.",
            ["Plugins.Misc.SinglePageCheckout.Fields.LayoutType"] = "Checkout layout",
            ["Plugins.Misc.SinglePageCheckout.Fields.LayoutType.Hint"] = "Choose which checkout template to use.",
            ["Plugins.Misc.SinglePageCheckout.Layout.Layout01"] = "Layout 01 - Single page (3 columns)",
            ["Plugins.Misc.SinglePageCheckout.Layout.Layout02"] = "Layout 02 - Stepped (2 columns)",
            ["Plugins.Misc.SinglePageCheckout.Summary"] = "Summary",
            ["Plugins.Misc.SinglePageCheckout.InYourCart"] = "In your cart ({0})",
            ["Plugins.Misc.SinglePageCheckout.EditCart"] = "Edit cart",
            ["Plugins.Misc.SinglePageCheckout.Step.Billing"] = "Billing",
            ["Plugins.Misc.SinglePageCheckout.Step.Shipping"] = "Shipping",
            ["Plugins.Misc.SinglePageCheckout.Step.Payment"] = "Payment",
            ["Plugins.Misc.SinglePageCheckout.ContinueTo.Shipping"] = "Continue to shipping",
            ["Plugins.Misc.SinglePageCheckout.ContinueTo.Payment"] = "Continue to payment",
            ["Plugins.Misc.SinglePageCheckout.UseNewAddress"] = "Use a new address",
            ["Plugins.Misc.SinglePageCheckout.Qty"] = "Qty",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowDiscountBox"] = "Show discount code box",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowDiscountBox.Hint"] = "Show the discount coupon entry box on the checkout page.",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowGiftCardBox"] = "Show gift card box",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowGiftCardBox.Hint"] = "Show the gift card code entry box on the checkout page.",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowCheckoutAttributes"] = "Show checkout attributes",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowCheckoutAttributes.Hint"] = "Show checkout attributes (e.g. gift wrapping) on the checkout page.",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowEstimateShipping"] = "Show estimate shipping & pickup",
            ["Plugins.Misc.SinglePageCheckout.Fields.ShowEstimateShipping.Hint"] = "Show the estimate-shipping and in-store pickup blocks on the checkout page.",
            ["Plugins.Misc.SinglePageCheckout.Configuration"] = "Single Page Checkout settings",
            ["Plugins.Misc.SinglePageCheckout.PlaceOrder"] = "Complete",
            ["Plugins.Misc.SinglePageCheckout.Submitting"] = "Processing your order...",
            ["Plugins.Misc.SinglePageCheckout.SectionFailed"] = "We couldn't process this step. Please review your details and try again."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<SinglePageCheckoutSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.SinglePageCheckout");

        await base.UninstallAsync();
    }

    #endregion
}
