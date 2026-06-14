using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.Banner.Models;

/// <summary>
/// Represents a single banner rendered on the public site
/// </summary>
public record PublicBannerModel : BaseNopModel
{
    public string PictureUrl { get; set; }
    public string LinkUrl { get; set; }
    public string AltText { get; set; }
}
