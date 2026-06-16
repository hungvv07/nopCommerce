using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.SinglePageCheckout.Models;

/// <summary>
/// A composite model that gathers every section nopCommerce normally renders across the
/// multiple checkout steps, so the whole checkout can be shown on a single page.
/// </summary>
public record SinglePageCheckoutModel : BaseNopModel
{
    /// <summary>The core one-page checkout model (billing address, captcha flags, etc.)</summary>
    public OnePageCheckoutModel OnePageCheckout { get; set; } = new();

    /// <summary>The shipping address form (null when shipping is not required)</summary>
    public CheckoutShippingAddressModel ShippingAddress { get; set; }

    /// <summary>Available shipping methods (null when shipping is not required)</summary>
    public CheckoutShippingMethodModel ShippingMethod { get; set; }

    /// <summary>Available payment methods</summary>
    public CheckoutPaymentMethodModel PaymentMethod { get; set; }

    /// <summary>Payment info form for the currently selected/first payment method (null when not required)</summary>
    public CheckoutPaymentInfoModel PaymentInfo { get; set; }

    /// <summary>Order summary: cart items, discount/gift-card boxes and checkout attributes</summary>
    public ShoppingCartModel Cart { get; set; } = new();

    /// <summary>Order totals (sub-total, shipping, tax, discount, total)</summary>
    public OrderTotalsModel OrderTotals { get; set; } = new();

    /// <summary>Estimate-shipping block (null when hidden or shipping not required)</summary>
    public EstimateShippingModel EstimateShipping { get; set; }

    public bool ShippingRequired { get; set; }
    public bool PaymentRequired { get; set; }

    public bool ShowDiscountBox { get; set; }
    public bool ShowGiftCardBox { get; set; }
    public bool ShowCheckoutAttributes { get; set; }
    public bool ShowEstimateShipping { get; set; }
}
