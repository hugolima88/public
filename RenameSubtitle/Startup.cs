using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RenameSubtitle.Startup))]
namespace RenameSubtitle
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
