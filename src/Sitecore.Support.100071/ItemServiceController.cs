using Item = Sitecore.Data.Items.Item;

namespace Sitecore.Services.Infrastructure.Sitecore.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.Routing;
    using Core;
    using Data;
    using Diagnostics;
    using Exceptions;
    using Handlers;
    using Model;
    using Infrastructure.Net.Http;

    public sealed class SupportItemServiceController : Web.Http.ServicesApiController
    {
        private const int PageSize = 10;

        private readonly IHandlerProvider _handlerProvider;

        private readonly Core.Diagnostics.ILogger _logger;

        public SupportItemServiceController(IHandlerProvider handlerProvider, Core.Diagnostics.ILogger logger)
        {
            this._handlerProvider = handlerProvider;
            this._logger = logger;
        }

        public SupportItemServiceController()
            : this(ApplicationContainer.ResolveHandlerProvider(), ApplicationContainer.ResolveLogger())
        {
        }

        [ActionName("GetItemByContentPath")]
        public Core.Model.ItemModel Get([ModelBinder(typeof(Web.Http.ModelBinding.GetItemByContentPathQueryModelBinder))] GetItemByContentPathQuery query)
        {
            IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemByContentPathHandler>();
            return (Core.Model.ItemModel)this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
        }

        [ActionName("DefaultAction")]
        public Core.Model.ItemModel Get([ModelBinder(typeof(Web.Http.ModelBinding.GetItemByIdQueryModelBinder))] GetItemByIdQuery query)
        {
            IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemByIdHandler>();
            return (Core.Model.ItemModel)this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
        }

        public Core.Model.ItemModel[] GetChildren([ModelBinder(typeof(Web.Http.ModelBinding.GetItemChildrenQueryModelBinder))] GetItemChildrenQuery query)
        {
            IItemRequestHandler handler = this._handlerProvider.GetHandler<GetItemChildrenHandler>();
            return (Core.Model.ItemModel[])this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
        }

        [HttpGet]
        public HttpResponseMessage QueryViaItem(Guid id, bool includeStandardTemplateFields = false, string fields = "", int page = 0, int pageSize = 10, string database = "", string language = "", string version = "")
        {
            if (pageSize < 1)
            {
                pageSize = 10;
            }
            SitecoreQueryViaItemQuery query = new SitecoreQueryViaItemQuery
            {
                Id = id,
                Database = database,
                Language = language,
                Version = version
            };
            IItemRequestHandler handler = new SupportQueryViaItemHandler(new ItemRepository(new SitecoreLogger()));
            Item[] items = (Item[])this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            Handlers.Query.FormatItemsQuery query2 = new Handlers.Query.FormatItemsQuery
            {
                Items = items,
                Fields = fields,
                IncludeStandardTemplateFields = includeStandardTemplateFields,
                Page = page,
                PageSize = pageSize,
                RequestMessage = base.Request,
                Controller = "ItemService-QueryViaItem",
                RouteValues = new
                {
                    includeStandardTemplateFields,
                    fields,
                    database,
                    id
                }
            };
            IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemsHandler>();
            object value = this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler2.Handle), query2);
            return base.Request.CreateResponse(HttpStatusCode.OK, value);
        }

        [HttpGet]
        public HttpResponseMessage Search(string term, bool includeStandardTemplateFields = false, string fields = "", int page = 0, int pageSize = 10, string database = "", string language = "", string sorting = "", string facet = "")
        {
            if (pageSize < 1)
            {
                pageSize = 10;
            }
            SearchQuery query = new SearchQuery
            {
                Term = term,
                Database = database,
                Language = language,
                Sorting = sorting,
                Page = page,
                PageSize = pageSize,
                Facet = facet
            };
            IItemRequestHandler handler = this._handlerProvider.GetHandler<SearchHandler>();
            ItemSearchResults itemSearchResults = (ItemSearchResults)this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemSearchResultsHandler>();
            Handlers.Query.FormatItemSearchResultsQuery query2 = new Handlers.Query.FormatItemSearchResultsQuery
            {
                ItemSearchResults = itemSearchResults,
                Fields = fields,
                IncludeStandardTemplateFields = includeStandardTemplateFields,
                Page = page,
                PageSize = pageSize,
                RequestMessage = base.Request,
                Controller = "ItemService-Search",
                RouteValues = new
                {
                    includeStandardTemplateFields,
                    fields,
                    database,
                    sorting,
                    term,
                    facet
                }
            };
            object value = this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler2.Handle), query2);
            return base.Request.CreateResponse(HttpStatusCode.OK, value);
        }

        [HttpGet]
        public HttpResponseMessage SearchViaItem([ModelBinder(typeof(Web.Http.ModelBinding.SearchViaItemQueryModelBinder))] SearchViaItemQuery query, int page = 0, int pageSize = 10)
        {
            if (pageSize < 1)
            {
                pageSize = 10;
            }
            IItemRequestHandler handler = this._handlerProvider.GetHandler<SearchViaItemHandler>();
            SearchViaItemQueryResponse searchViaItemQueryResponse = (SearchViaItemQueryResponse)this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            IItemRequestHandler handler2 = this._handlerProvider.GetHandler<FormatItemSearchResultsHandler>();
            Handlers.Query.FormatItemSearchResultsQuery query2 = new Handlers.Query.FormatItemSearchResultsQuery
            {
                ItemSearchResults = searchViaItemQueryResponse.ItemSearchResults,
                Fields = searchViaItemQueryResponse.SearchRequest.Fields,
                IncludeStandardTemplateFields = searchViaItemQueryResponse.SearchRequest.IncludeStandardTemplateFields,
                Page = page,
                PageSize = pageSize,
                RequestMessage = base.Request,
                Controller = "ItemService-SearchViaItem",
                RouteValues = new
                {
                    includeStandardTemplateFields = searchViaItemQueryResponse.SearchRequest.IncludeStandardTemplateFields,
                    fields = searchViaItemQueryResponse.SearchRequest.Fields,
                    database = searchViaItemQueryResponse.SearchRequest.Database,
                    sorting = searchViaItemQueryResponse.SearchRequest.Sorting,
                    Term = query.Term,
                    facet = searchViaItemQueryResponse.SearchRequest.Facet
                }
            };
            object value = this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler2.Handle), query2);
            return base.Request.CreateResponse(HttpStatusCode.OK, value);
        }

        [ActionName("DefaultAction")]
        public HttpResponseMessage Post(string path, [FromBody] Core.Model.ItemModel itemModel, string database = "", string language = "")
        {
            CreateItemCommand query = new CreateItemCommand
            {
                Path = path,
                ItemModel = itemModel,
                Database = database,
                Language = language
            };
            IItemRequestHandler handler = this._handlerProvider.GetHandler<CreateItemHandler>();
            CreateItemResponse createItemResponse = (CreateItemResponse)this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
            UrlHelper urlHelper = new UrlHelper(base.Request);
            string value = urlHelper.Link("ItemService", new
            {
                id = createItemResponse.ItemId,
                Database = createItemResponse.Database,
                Language = createItemResponse.Language
            });
            if (!string.IsNullOrEmpty(value))
            {
                httpResponseMessage.Headers.Add("Location", value);
            }
            return httpResponseMessage;
        }

        [ActionName("DefaultAction")]
        public HttpResponseMessage Delete(Guid id, string database = "", string language = "")
        {
            DeleteItemCommand query = new DeleteItemCommand
            {
                Id = id,
                Database = database,
                Language = language
            };
            IItemRequestHandler handler = this._handlerProvider.GetHandler<DeleteItemHandler>();
            this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        [ActionName("DefaultAction"), HttpPatch]
        public HttpResponseMessage Update(Guid id, [FromBody] Core.Model.ItemModel itemModel, string database = "", string language = "", string version = "")
        {
            UpdateItemCommand query = new UpdateItemCommand
            {
                Id = id,
                ItemModel = itemModel,
                Database = database,
                Language = language,
                Version = version
            };
            IItemRequestHandler handler = this._handlerProvider.GetHandler<UpdateItemHandler>();
            this.ProcessRequest<object>(new Func<HandlerRequest, object>(handler.Handle), query);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        private T ProcessRequest<T>(Func<HandlerRequest, T> handler, HandlerRequest query)
        {
            T result;
            try
            {
                result = handler(query);
            }
            catch (ItemNotFoundException ex)
            {
                throw new Web.Http.ApiControllerException(HttpStatusCode.NotFound, "Item Not Found", ex.Message);
            }
            catch (ArgumentException ex2)
            {
                throw new Web.Http.ApiControllerException(HttpStatusCode.BadRequest, ex2.Message, "");
            }
            catch (ApplicationException ex3)
            {
                throw new Web.Http.ApiControllerException(HttpStatusCode.ServiceUnavailable, ex3.Message, "");
            }
            catch (Exception ex4)
            {
                if (ex4.IsAccessViolation())
                {
                    this._logger.Warn(string.Format("Access Denied: {0}\n\nRequest from {1}", ex4.Message, base.Request.GetClientIpAddress()));
                    throw new Web.Http.ApiControllerException(HttpStatusCode.Forbidden);
                }
                this._logger.Error(ex4.ToString());
                throw new Web.Http.ApiControllerException(HttpStatusCode.InternalServerError);
            }
            return result;
        }
    }
}