using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HyprsoftProjectsMobileService.Startup))]

namespace HyprsoftProjectsMobileService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureMobileApp(app);
        }
    }
}