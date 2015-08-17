using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StorIconGeneratorWeb.Startup))]
namespace StorIconGeneratorWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
