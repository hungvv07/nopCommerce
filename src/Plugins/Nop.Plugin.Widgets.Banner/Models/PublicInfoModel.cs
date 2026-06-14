using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.Banner.Models;

/// <summary>
/// Represents the public banner widget model (all active banners for one zone)
/// </summary>
public record PublicInfoModel : BaseNopModel
{
    public List<PublicBannerModel> Banners { get; set; } = new();
}
