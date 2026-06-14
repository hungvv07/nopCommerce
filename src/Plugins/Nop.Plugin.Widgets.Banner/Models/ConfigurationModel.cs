using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.Banner.Models;

/// <summary>
/// Represents the banner widget configuration (list) model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        BannerSearchModel = new BannerSearchModel();
    }

    /// <summary>
    /// Gets or sets the search/grid model that hosts the banner list
    /// </summary>
    public BannerSearchModel BannerSearchModel { get; set; }
}
