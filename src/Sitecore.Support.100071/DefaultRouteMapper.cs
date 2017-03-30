using IMapRoutes = Sitecore.Services.Infrastructure.Web.Http.IMapRoutes;

namespace Sitecore.Support.Services.Infrastructure.Web.Http
{
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class DefaultRouteMapper : IMapRoutes
    {
        private readonly string _routeBase;
        private const string GuidRegex = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$";

        public DefaultRouteMapper(string routeBase)
        {
            this._routeBase = routeBase ?? "";
        }

        public void MapRoutes(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("ItemService-QueryViaItem", this._routeBase + "item/{id}/query", new
            {
                controller = "SupportItemService",
                action = "QueryViaItem"
            }, new { id = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });
            config.Routes.MapHttpRoute("ItemService-Search", this._routeBase + "item/search", new
            {
                controller = "ItemService",
                action = "Search"
            });
            config.Routes.MapHttpRoute("ItemService-SearchViaItem", this._routeBase + "item/{id}/search", new
            {
                controller = "ItemService",
                action = "SearchViaItem"
            }, new { id = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });
            config.Routes.MapHttpRoute("ItemService-Children", this._routeBase + "item/{id}/children", new
            {
                controller = "ItemService",
                action = "GetChildren"
            }, new { id = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });
            config.Routes.MapHttpRoute("ItemService", this._routeBase + "item/{id}", new
            {
                controller = "ItemService",
                action = "DefaultAction"
            }, new { id = @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$" });
            config.Routes.MapHttpRoute("ItemService-ContentPath", this._routeBase + "item", new
            {
                controller = "ItemService",
                action = "GetItemByContentPath"
            });
            config.Routes.MapHttpRoute("ItemService-Path", this._routeBase + "item/{*path}", new { controller = "ItemService" });
            config.Routes.MapHttpRoute("EntityService", this._routeBase + "{namespace}/{controller}/{id}/{action}", new
            {
                id = RouteParameter.Optional,
                action = "DefaultAction"
            });
        }

        public void MapRoutes(RouteCollection routes)
        {
            routes.MapRoute("MetaDataScript", this._routeBase + "script/metadata", new
            {
                controller = "MetaDataScript",
                action = "GetScripts"
            }, new string[] { "Sitecore.Services.Infrastructure.Sitecore.Mvc" });
            routes.MapRoute("Authentication", this._routeBase + "auth/{action}", new { controller = "ServicesAuthentication" }, new string[] { "Sitecore.Services.Infrastructure.Sitecore.Mvc" });
        }

        public static class RouteName
        {
            public const string Authentication = "Authentication";

            public static class EntityService
            {
                public const string IdAction = "EntityService";
                public const string MetaDataScript = "MetaDataScript";
            }

            public static class ItemService
            {
                public const string Children = "ItemService-Children";
                public const string ContentPath = "ItemService-ContentPath";
                public const string Default = "ItemService";
                public const string Path = "ItemService-Path";
                public const string QueryViaItem = "ItemService-QueryViaItem";
                public const string Search = "ItemService-Search";
                public const string SearchViaItem = "ItemService-SearchViaItem";
            }
        }
    }
}
