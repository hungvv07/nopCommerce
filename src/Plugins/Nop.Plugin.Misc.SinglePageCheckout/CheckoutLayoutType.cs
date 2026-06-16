namespace Nop.Plugin.Misc.SinglePageCheckout;

/// <summary>
/// Represents the selectable single-page checkout layout templates
/// </summary>
public enum CheckoutLayoutType
{
    /// <summary>
    /// Flat three-column layout (billing/shipping | methods | summary), everything visible at once
    /// </summary>
    Layout01 = 0,

    /// <summary>
    /// Two-column stepped layout: numbered Billing/Shipping/Payment steps on the left, sticky summary + cart on the right
    /// </summary>
    Layout02 = 1
}
