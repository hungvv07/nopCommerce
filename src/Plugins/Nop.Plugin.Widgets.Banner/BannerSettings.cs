using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.Banner;

/// <summary>
/// Represents settings of the Banner widget plugin
/// </summary>
public class BannerSettings : ISettings
{
    /// <summary>
    /// Gets or sets the configured banners, serialized as a JSON array of
    /// <see cref="Domain.BannerItem"/>.
    /// </summary>
    public string Banners { get; set; }
}
