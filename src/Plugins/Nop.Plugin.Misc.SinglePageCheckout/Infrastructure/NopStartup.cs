using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Misc.SinglePageCheckout.Infrastructure;

/// <summary>
/// Registers the services required by the Single Page Checkout plugin
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //override the core one-page checkout view with the plugin's single-page layout
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new SinglePageCheckoutViewLocationExpander());
        });
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    //run after nopCommerce core startup so the framework expanders are already registered
    public int Order => 3000;
}
