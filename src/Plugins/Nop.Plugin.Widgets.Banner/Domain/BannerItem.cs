namespace Nop.Plugin.Widgets.Banner.Domain;

/// <summary>
/// Represents a single configured banner. Persisted as an item in the JSON list
/// stored in <see cref="BannerSettings.Banners"/>.
/// </summary>
public class BannerItem
{
    /// <summary>
    /// Gets or sets the banner identifier (unique within the list)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the uploaded banner picture
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Gets or sets the public widget zone this banner is shown in
    /// </summary>
    public string WidgetZone { get; set; }

    /// <summary>
    /// Gets or sets the URL the banner links to (optional; empty = not clickable)
    /// </summary>
    public string LinkUrl { get; set; }

    /// <summary>
    /// Gets or sets the alternate text added to the banner image
    /// </summary>
    public string AltText { get; set; }

    /// <summary>
    /// Gets or sets the UTC date/time from which the banner starts showing (optional)
    /// </summary>
    public DateTime? StartDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC date/time after which the banner stops showing (optional)
    /// </summary>
    public DateTime? EndDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the display order within a widget zone (ascending)
    /// </summary>
    public int DisplayOrder { get; set; }
}
