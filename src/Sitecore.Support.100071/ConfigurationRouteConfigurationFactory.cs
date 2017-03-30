using IMapRoutes = Sitecore.Services.Infrastructure.Web.Http.IMapRoutes;

namespace Sitecore.Services.Infrastructure.Configuration
{
    using System;

    public class SupportConfigurationRouteConfigurationFactory : ConfigurationFactoryBase<IMapRoutes>
    {
        public SupportConfigurationRouteConfigurationFactory(Core.Configuration.IServicesConfiguration servicesConfiguration, Core.Diagnostics.ILogger logger)
            : base(servicesConfiguration, logger)
        {
        }

        private IMapRoutes DefaultObjectBuilder()
        {
            return new Support.Services.Infrastructure.Web.Http.DefaultRouteMapper(base.ServicesConfiguration.Configuration.Services.Routes.RouteBase);
        }

        public override IMapRoutes Instance
        {
            get
            {
                return base.CreateInstance(s => s.Services.Routes.RouteMapper, new Func<IMapRoutes>(this.DefaultObjectBuilder));
            }
        }
    }
}