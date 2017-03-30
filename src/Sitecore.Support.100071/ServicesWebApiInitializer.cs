using PipelineArgs = Sitecore.Pipelines.PipelineArgs;
using SupportApplicationContainer = Sitecore.Services.Infrastructure.Sitecore.SupportApplicationContainer;

namespace Sitecore.Support.Services.Infrastructure.Sitecore.Pipelines
{
    using System.Web.Http;
    using System.Web.Routing;

    public class ServicesWebApiInitializer
    {
        public void Process(PipelineArgs args)
        {
            new SupportApplicationContainer().ResolveServicesWebApiConfiguration().Configure(GlobalConfiguration.Configuration, RouteTable.Routes);
        }
    }
}