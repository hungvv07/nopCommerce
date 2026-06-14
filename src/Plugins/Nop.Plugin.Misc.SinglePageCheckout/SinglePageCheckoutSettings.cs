using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.SinglePageCheckout;

/// <summary>
/// Represents the settings of the Single Page Checkout plugin
/// </summary>
public class SinglePageCheckoutSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the single-page checkout layout is enabled
    /// (when disabled, nopCommerce renders its default one-page/multi-step checkout)
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the discount coupon box is shown
    /// </summary>
    public bool ShowDiscountBox { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the gift card box is shown
    /// </summary>
    public bool ShowGiftCardBox { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether checkout attributes (e.g. gift wrapping) are shown
    /// </summary>
    public bool ShowCheckoutAttributes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the estimate-shipping and in-store pickup blocks are shown
    /// </summary>
    public bool ShowEstimateShipping { get; set; }
}
