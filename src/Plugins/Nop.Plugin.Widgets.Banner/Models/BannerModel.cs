using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Banner.Models;

/// <summary>
/// Represents a banner model (used for the create/edit form and as a grid row)
/// </summary>
public record BannerModel : BaseNopModel
{
    public BannerModel()
    {
        AvailableWidgetZones = new List<SelectListItem>();
    }

    public int Id { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.PictureId")]
    public int PictureId { get; set; }

    /// <summary>
    /// Resolved picture URL, used for display in the admin grid
    /// </summary>
    public string PictureUrl { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.WidgetZone")]
    public string WidgetZone { get; set; }
    public IList<SelectListItem> AvailableWidgetZones { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.LinkUrl")]
    public string LinkUrl { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.AltText")]
    public string AltText { get; set; }

    [UIHint("DateTimeNullable")]
    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.StartDateUtc")]
    public DateTime? StartDateUtc { get; set; }

    [UIHint("DateTimeNullable")]
    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.EndDateUtc")]
    public DateTime? EndDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.Banner.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
}
