using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(iTrade.Startup))]
namespace iTrade
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
