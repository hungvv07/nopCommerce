using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.Banner.Domain;
using Nop.Plugin.Widgets.Banner.Models;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.Banner.Components;

public class WidgetsBannerViewComponent : NopViewComponent
{
    #region Fields

    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public WidgetsBannerViewComponent(IPictureService pictureService,
        ISettingService settingService)
    {
        _pictureService = pictureService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var settings = await _settingService.LoadSettingAsync<BannerSettings>();
        if (string.IsNullOrEmpty(settings.Banners))
            return Content(string.Empty);

        var banners = JsonSerializer.Deserialize<List<BannerItem>>(settings.Banners) ?? new List<BannerItem>();

        //only banners assigned to this zone, currently within their schedule, in order
        var nowUtc = DateTime.UtcNow;
        var activeBanners = banners
            .Where(b => b.WidgetZone == widgetZone && b.PictureId != 0)
            .Where(b => !b.StartDateUtc.HasValue || nowUtc >= b.StartDateUtc.Value)
            .Where(b => !b.EndDateUtc.HasValue || nowUtc <= b.EndDateUtc.Value)
            .OrderBy(b => b.DisplayOrder)
            .ToList();

        if (!activeBanners.Any())
            return Content(string.Empty);

        var model = new PublicInfoModel();
        foreach (var banner in activeBanners)
        {
            var pictureUrl = await _pictureService.GetPictureUrlAsync(banner.PictureId, showDefaultPicture: false);
            if (string.IsNullOrEmpty(pictureUrl))
                continue;

            model.Banners.Add(new PublicBannerModel
            {
                PictureUrl = pictureUrl,
                LinkUrl = banner.LinkUrl,
                AltText = banner.AltText
            });
        }

        if (!model.Banners.Any())
            return Content(string.Empty);

        return View("~/Plugins/Widgets.Banner/Views/PublicInfo.cshtml", model);
    }

    #endregion
}
