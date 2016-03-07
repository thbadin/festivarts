using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FestivArts.Startup))]
namespace FestivArts
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
