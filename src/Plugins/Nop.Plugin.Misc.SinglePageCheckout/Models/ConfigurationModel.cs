using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.SinglePageCheckout.Models;

public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        AvailableLayouts = new List<SelectListItem>();
    }

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.LayoutType")]
    public int LayoutTypeId { get; set; }
    public bool LayoutTypeId_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableLayouts { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.ShowDiscountBox")]
    public bool ShowDiscountBox { get; set; }
    public bool ShowDiscountBox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.ShowGiftCardBox")]
    public bool ShowGiftCardBox { get; set; }
    public bool ShowGiftCardBox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.ShowCheckoutAttributes")]
    public bool ShowCheckoutAttributes { get; set; }
    public bool ShowCheckoutAttributes_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SinglePageCheckout.Fields.ShowEstimateShipping")]
    public bool ShowEstimateShipping { get; set; }
    public bool ShowEstimateShipping_OverrideForStore { get; set; }
}
