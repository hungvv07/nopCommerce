using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.SinglePageCheckout.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.SinglePageCheckout.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class MiscSinglePageCheckoutController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public MiscSinglePageCheckoutController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<SinglePageCheckoutSettings>(storeScope);

        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            LayoutTypeId = (int)settings.LayoutType,
            ShowDiscountBox = settings.ShowDiscountBox,
            ShowGiftCardBox = settings.ShowGiftCardBox,
            ShowCheckoutAttributes = settings.ShowCheckoutAttributes,
            ShowEstimateShipping = settings.ShowEstimateShipping,
            ActiveStoreScopeConfiguration = storeScope
        };

        model.AvailableLayouts.Add(new SelectListItem
        {
            Value = ((int)CheckoutLayoutType.Layout01).ToString(),
            Text = await _localizationService.GetResourceAsync("Plugins.Misc.SinglePageCheckout.Layout.Layout01"),
            Selected = settings.LayoutType == CheckoutLayoutType.Layout01
        });
        model.AvailableLayouts.Add(new SelectListItem
        {
            Value = ((int)CheckoutLayoutType.Layout02).ToString(),
            Text = await _localizationService.GetResourceAsync("Plugins.Misc.SinglePageCheckout.Layout.Layout02"),
            Selected = settings.LayoutType == CheckoutLayoutType.Layout02
        });

        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Enabled, storeScope);
            model.LayoutTypeId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.LayoutType, storeScope);
            model.ShowDiscountBox_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowDiscountBox, storeScope);
            model.ShowGiftCardBox_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowGiftCardBox, storeScope);
            model.ShowCheckoutAttributes_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowCheckoutAttributes, storeScope);
            model.ShowEstimateShipping_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowEstimateShipping, storeScope);
        }

        return View("~/Plugins/Misc.SinglePageCheckout/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<SinglePageCheckoutSettings>(storeScope);

        settings.Enabled = model.Enabled;
        settings.LayoutType = (CheckoutLayoutType)model.LayoutTypeId;
        settings.ShowDiscountBox = model.ShowDiscountBox;
        settings.ShowGiftCardBox = model.ShowGiftCardBox;
        settings.ShowCheckoutAttributes = model.ShowCheckoutAttributes;
        settings.ShowEstimateShipping = model.ShowEstimateShipping;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.LayoutType, model.LayoutTypeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowDiscountBox, model.ShowDiscountBox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowGiftCardBox, model.ShowGiftCardBox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowCheckoutAttributes, model.ShowCheckoutAttributes_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowEstimateShipping, model.ShowEstimateShipping_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}
