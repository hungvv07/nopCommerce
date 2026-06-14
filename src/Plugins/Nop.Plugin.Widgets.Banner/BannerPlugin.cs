using System.Text.Json;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.Banner.Components;
using Nop.Plugin.Widgets.Banner.Domain;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;

namespace Nop.Plugin.Widgets.Banner;

/// <summary>
/// Represents the banner widget
/// </summary>
public class BannerPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public BannerPlugin(ILocalizationService localizationService,
        IPictureService pictureService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _pictureService = pictureService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        //register the widget in every selectable zone; the view component renders
        //only the banners assigned to the requested zone (and returns nothing
        //otherwise). This keeps newly assigned zones working without an app restart.
        return Task.FromResult<IList<string>>(WidgetsBannerDefaults.SelectableWidgetZones.ToList());
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/WidgetsBanner/Configure";
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(WidgetsBannerViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        await _settingService.SaveSettingAsync(new BannerSettings
        {
            Banners = JsonSerializer.Serialize(new List<BannerItem>())
        });

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(WidgetsBannerDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(WidgetsBannerDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.Banner.Fields.PictureId"] = "Banner image",
            ["Plugins.Widgets.Banner.Fields.PictureId.Hint"] = "Upload the banner image to display on the public site.",
            ["Plugins.Widgets.Banner.Fields.WidgetZone"] = "Widget zone",
            ["Plugins.Widgets.Banner.Fields.WidgetZone.Hint"] = "Choose the public page location where this banner is displayed.",
            ["Plugins.Widgets.Banner.Fields.LinkUrl"] = "Link URL",
            ["Plugins.Widgets.Banner.Fields.LinkUrl.Hint"] = "Enter a URL to make the banner clickable. Leave empty for a non-clickable image.",
            ["Plugins.Widgets.Banner.Fields.AltText"] = "Image alternate text",
            ["Plugins.Widgets.Banner.Fields.AltText.Hint"] = "Enter alternate text that will be added to the banner image.",
            ["Plugins.Widgets.Banner.Fields.StartDateUtc"] = "Start date (UTC)",
            ["Plugins.Widgets.Banner.Fields.StartDateUtc.Hint"] = "The banner starts showing from this UTC date/time. Leave empty to show immediately.",
            ["Plugins.Widgets.Banner.Fields.EndDateUtc"] = "End date (UTC)",
            ["Plugins.Widgets.Banner.Fields.EndDateUtc.Hint"] = "The banner stops showing after this UTC date/time. Leave empty to show indefinitely.",
            ["Plugins.Widgets.Banner.Fields.DisplayOrder"] = "Display order",
            ["Plugins.Widgets.Banner.Fields.DisplayOrder.Hint"] = "Banners in the same widget zone are shown in ascending display order.",
            ["Plugins.Widgets.Banner.AddNew"] = "Add new banner"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //remove uploaded pictures
        var settings = await _settingService.LoadSettingAsync<BannerSettings>();
        if (!string.IsNullOrEmpty(settings.Banners))
        {
            var banners = JsonSerializer.Deserialize<List<BannerItem>>(settings.Banners) ?? new List<BannerItem>();
            foreach (var banner in banners.Where(b => b.PictureId > 0))
            {
                var picture = await _pictureService.GetPictureByIdAsync(banner.PictureId);
                if (picture != null)
                    await _pictureService.DeletePictureAsync(picture);
            }
        }

        //settings
        await _settingService.DeleteSettingAsync<BannerSettings>();
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(WidgetsBannerDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(WidgetsBannerDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.Banner");

        await base.UninstallAsync();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}
