using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.Banner.Domain;
using Nop.Plugin.Widgets.Banner.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.Banner.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class WidgetsBannerController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public WidgetsBannerController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPictureService pictureService,
        ISettingService settingService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _pictureService = pictureService;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<List<BannerItem>> GetBannersAsync()
    {
        var settings = await _settingService.LoadSettingAsync<BannerSettings>();
        if (string.IsNullOrEmpty(settings.Banners))
            return new List<BannerItem>();

        return JsonSerializer.Deserialize<List<BannerItem>>(settings.Banners) ?? new List<BannerItem>();
    }

    protected virtual async Task SaveBannersAsync(List<BannerItem> banners)
    {
        var settings = await _settingService.LoadSettingAsync<BannerSettings>();
        settings.Banners = JsonSerializer.Serialize(banners);
        await _settingService.SaveSettingAsync(settings);
        await _settingService.ClearCacheAsync();
    }

    protected virtual void PrepareAvailableWidgetZones(BannerModel model)
    {
        model.AvailableWidgetZones = WidgetsBannerDefaults.SelectableWidgetZones
            .Select(zone => new SelectListItem { Text = zone, Value = zone })
            .ToList();
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public IActionResult Configure()
    {
        var model = new ConfigurationModel();
        model.BannerSearchModel.SetGridPageSize();

        return View("~/Plugins/Widgets.Banner/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> List(BannerSearchModel searchModel)
    {
        var banners = (await GetBannersAsync())
            .OrderBy(b => b.WidgetZone)
            .ThenBy(b => b.DisplayOrder)
            .ToList();

        var pagedBanners = banners.ToPagedList(searchModel);

        var model = await new BannerListModel().PrepareToGridAsync(searchModel, pagedBanners, () =>
        {
            return pagedBanners.SelectAwait(async banner => new BannerModel
            {
                Id = banner.Id,
                PictureId = banner.PictureId,
                PictureUrl = await _pictureService.GetPictureUrlAsync(banner.PictureId, 100, showDefaultPicture: false),
                WidgetZone = banner.WidgetZone,
                LinkUrl = banner.LinkUrl,
                AltText = banner.AltText,
                StartDateUtc = banner.StartDateUtc,
                EndDateUtc = banner.EndDateUtc,
                DisplayOrder = banner.DisplayOrder
            });
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Create()
    {
        var banners = await GetBannersAsync();
        var model = new BannerModel
        {
            WidgetZone = WidgetsBannerDefaults.DefaultWidgetZone,
            DisplayOrder = banners.Any() ? banners.Max(b => b.DisplayOrder) + 1 : 1
        };
        PrepareAvailableWidgetZones(model);

        return View("~/Plugins/Widgets.Banner/Views/Create.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Create(BannerModel model)
    {
        if (ModelState.IsValid)
        {
            var banners = await GetBannersAsync();
            banners.Add(new BannerItem
            {
                Id = banners.Any() ? banners.Max(b => b.Id) + 1 : 1,
                PictureId = model.PictureId,
                WidgetZone = model.WidgetZone,
                LinkUrl = model.LinkUrl,
                AltText = model.AltText,
                StartDateUtc = model.StartDateUtc,
                EndDateUtc = model.EndDateUtc,
                DisplayOrder = model.DisplayOrder
            });
            await SaveBannersAsync(banners);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        //something failed, redisplay form
        PrepareAvailableWidgetZones(model);

        return View("~/Plugins/Widgets.Banner/Views/Create.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Edit(int id)
    {
        var banner = (await GetBannersAsync()).FirstOrDefault(b => b.Id == id);
        if (banner == null)
            return RedirectToAction("Configure");

        var model = new BannerModel
        {
            Id = banner.Id,
            PictureId = banner.PictureId,
            WidgetZone = banner.WidgetZone,
            LinkUrl = banner.LinkUrl,
            AltText = banner.AltText,
            StartDateUtc = banner.StartDateUtc,
            EndDateUtc = banner.EndDateUtc,
            DisplayOrder = banner.DisplayOrder
        };
        PrepareAvailableWidgetZones(model);

        return View("~/Plugins/Widgets.Banner/Views/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Edit(BannerModel model)
    {
        var banners = await GetBannersAsync();
        var banner = banners.FirstOrDefault(b => b.Id == model.Id);
        if (banner == null)
            return RedirectToAction("Configure");

        if (ModelState.IsValid)
        {
            //if the picture was replaced, remove the previous one
            if (banner.PictureId != 0 && banner.PictureId != model.PictureId)
            {
                var oldPicture = await _pictureService.GetPictureByIdAsync(banner.PictureId);
                if (oldPicture != null)
                    await _pictureService.DeletePictureAsync(oldPicture);
            }

            banner.PictureId = model.PictureId;
            banner.WidgetZone = model.WidgetZone;
            banner.LinkUrl = model.LinkUrl;
            banner.AltText = model.AltText;
            banner.StartDateUtc = model.StartDateUtc;
            banner.EndDateUtc = model.EndDateUtc;
            banner.DisplayOrder = model.DisplayOrder;
            await SaveBannersAsync(banners);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return RedirectToAction("Configure");
        }

        //something failed, redisplay form
        PrepareAvailableWidgetZones(model);

        return View("~/Plugins/Widgets.Banner/Views/Edit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var banners = await GetBannersAsync();
        var banner = banners.FirstOrDefault(b => b.Id == id);
        if (banner != null)
        {
            banners.Remove(banner);
            await SaveBannersAsync(banners);

            if (banner.PictureId > 0)
            {
                var picture = await _pictureService.GetPictureByIdAsync(banner.PictureId);
                if (picture != null)
                    await _pictureService.DeletePictureAsync(picture);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
        }

        return RedirectToAction("Configure");
    }

    #endregion
}
