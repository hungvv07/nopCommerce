using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.OrderItemsRowDetails.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Widgets.OrderItemsRowDetails;

/// <summary>
/// Represents the Order items row details widget. It enhances the admin order
/// list grid with an expandable row that AJAX-loads the order's products.
/// </summary>
public class OrderItemsRowDetailsPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public OrderItemsRowDetailsPlugin(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
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
        return Task.FromResult<IList<string>>(new List<string> { WidgetsOrderItemsRowDetailsDefaults.WidgetZone });
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(OrderItemsRowDetailsViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //activate the widget
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(WidgetsOrderItemsRowDetailsDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(WidgetsOrderItemsRowDetailsDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.OrderItemsRowDetails.ShowProducts"] = "Show products",
            ["Plugins.Widgets.OrderItemsRowDetails.HideProducts"] = "Hide products",
            ["Plugins.Widgets.OrderItemsRowDetails.Loading"] = "Loading products...",
            ["Plugins.Widgets.OrderItemsRowDetails.LoadError"] = "Could not load the products for this order.",
            ["Plugins.Widgets.OrderItemsRowDetails.NoProducts"] = "This order has no products."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //deactivate the widget
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(WidgetsOrderItemsRowDetailsDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(WidgetsOrderItemsRowDetailsDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.OrderItemsRowDetails");

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
