using HttpRequestOrigin = Sitecore.Services.Infrastructure.Net.Http.HttpRequestOrigin;
using IMapRoutes = Sitecore.Services.Infrastructure.Web.Http.IMapRoutes;

namespace Sitecore.Services.Infrastructure.Sitecore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Filters;
    using Configuration;
    using Core;
    using Diagnostics;
    using Handlers;
    using Reflection;
    using Security;

    public class SupportApplicationContainer
    {
        private Dictionary<Type, Func<object>> _builderMethods;

        private readonly SitecoreLogger _logger;

        private readonly ServicesSettingsConfigurationProvider _servicesSettings;

        private readonly ITypeProvider _restrictedControllerProvider;

        private readonly HttpRequestOrigin _httpRequestOrigin;

        private static Assembly[] _siteAssemblies;

        public SupportApplicationContainer()
        {
            this._servicesSettings = new ServicesSettingsConfigurationProvider();
            this._logger = new SitecoreLogger();
            this._restrictedControllerProvider = new RestrictedControllerProvider();
            this._httpRequestOrigin = new HttpRequestOrigin();
        }

        public static Core.ComponentModel.DataAnnotations.IEntityValidator ResolveEntityValidator()
        {
            return new Core.ComponentModel.DataAnnotations.EntityValidator();
        }

        public static Infrastructure.Services.IMetaDataBuilder ResolveMetaDataBuilder()
        {
            List<string> genericTypesToMapToArray = new List<string>
            {
                "List`1",
                "IEnumerable`1"
            };
            Core.MetaData.EntityParser entityParser = new Core.MetaData.EntityParser(new JavascriptTypeMapper(), genericTypesToMapToArray, SupportApplicationContainer.ResolveValidationMetaDataProvider());
            return new Infrastructure.Services.MetaDataBuilder(entityParser);
        }

        private static Core.MetaData.IValidationMetaDataProvider ResolveValidationMetaDataProvider()
        {
            Core.Diagnostics.ILogger logger = SupportApplicationContainer.ResolveLogger();
            return new Core.MetaData.AssemblyScannerValidationMetaDataProvider(new Core.MetaData.ValidationMetaDataTypeProvider(SupportApplicationContainer.GetSiteAssemblies()), logger);
        }

        public virtual Web.Http.IHttpConfiguration ResolveServicesWebApiConfiguration()
        {
            this._builderMethods = this.CreateBuilderMapping();
            NamespaceQualifiedUniqueNameGenerator controllerNameGenerator = new NamespaceQualifiedUniqueNameGenerator(DefaultHttpControllerSelector.ControllerSuffix);
            Web.Http.Dispatcher.NamespaceHttpControllerSelector httpControllerSelector = new Web.Http.Dispatcher.NamespaceHttpControllerSelector(GlobalConfiguration.Configuration, controllerNameGenerator);
            IMapRoutes instance = new Infrastructure.Configuration.SupportConfigurationRouteConfigurationFactory(this._servicesSettings, this._logger).Instance;
            Core.Configuration.ConfigurationFilterProvider configurationFilterProvider = new Core.Configuration.ConfigurationFilterProvider(new FilterProvider(SupportApplicationContainer.GetSiteAssemblies()), new FilterTypeNames(this._logger).Types);
            IEnumerable<IFilter> filters = this.GetFilters(configurationFilterProvider.Types);
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[]
            {
                new Web.Http.Formatting.BrowserJsonFormatter()
            };
            return new Web.Http.ServicesHttpConfiguration(httpControllerSelector, instance, filters.ToArray<IFilter>(), formatters, this._logger);
        }

        private static Assembly[] GetSiteAssemblies()
        {
            Assembly[] arg_19_0;
            if ((arg_19_0 = SupportApplicationContainer._siteAssemblies) == null)
            {
                arg_19_0 = (SupportApplicationContainer._siteAssemblies = AppDomain.CurrentDomain.GetAssemblies());
            }
            return arg_19_0;
        }

        private IEnumerable<IFilter> GetFilters(IEnumerable<Type> filterTypes)
        {
            foreach (Type current in filterTypes)
            {
                if (this._builderMethods.ContainsKey(current))
                {
                    yield return (IFilter)this._builderMethods[current]();
                }
                else
                {
                    IFilter filter = (IFilter)this.CreateInstance(current);
                    if (filter != null)
                    {
                        yield return filter;
                    }
                    else
                    {
                        this._logger.Error("Filter ({0}) instance cannot be created, missing builder mapping", new object[]
                        {
                            current
                        });
                    }
                }
            }
            yield break;
        }

        private object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                this._logger.Warn("Failed to create instance of {0}, exception details {1}", new object[]
                {
                    ex.Message
                });
            }
            return null;
        }

        protected virtual Dictionary<Type, Func<object>> CreateBuilderMapping()
        {
            return new Dictionary<Type, Func<object>>
            {
                {
                    typeof(Web.Http.Filters.AnonymousUserFilter),
                    new Func<object>(this.BuildAnonymousUserFilter)
                },
                {
                    typeof(Web.Http.Filters.SecurityPolicyAuthorisationFilter),
                    new Func<object>(this.BuildSecurityPolicyAuthorisationFilter)
                },
                {
                    typeof(Web.Http.Filters.LoggingExceptionFilter),
                    new Func<object>(this.BuildLoggingExceptionFilter)
                }
            };
        }

        private object BuildAnonymousUserFilter()
        {
            return new Web.Http.Filters.AnonymousUserFilter(new UserService(), this._servicesSettings, this._restrictedControllerProvider, this._logger, this._httpRequestOrigin);
        }

        private object BuildSecurityPolicyAuthorisationFilter()
        {
            return new Web.Http.Filters.SecurityPolicyAuthorisationFilter(new Infrastructure.Configuration.ConfigurationSecurityPolicyFactory(this._servicesSettings, this._logger), this._logger, this._httpRequestOrigin, new AllowedControllerTypeNames(this._logger).Types);
        }

        protected object BuildLoggingExceptionFilter()
        {
            return new Web.Http.Filters.LoggingExceptionFilter(this._logger);
        }

        public static IHandlerProvider ResolveHandlerProvider()
        {
            return new HandlerProvider();
        }

        public static Core.Diagnostics.ILogger ResolveLogger()
        {
            return new SitecoreLogger();
        }
    }
}