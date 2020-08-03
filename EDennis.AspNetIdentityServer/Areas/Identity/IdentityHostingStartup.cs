using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(EDennis.AspNetIdentityServer.Areas.Identity.IdentityHostingStartup))]
namespace EDennis.AspNetIdentityServer.Areas.Identity {
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}