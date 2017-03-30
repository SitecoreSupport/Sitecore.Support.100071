using Database = Sitecore.Data.Database;
using Factory = Sitecore.Configuration.Factory;
using ID = Sitecore.Data.ID;
using Item = Sitecore.Data.Items.Item;
using Q = Sitecore.Data.Query.Query;


namespace Sitecore.Services.Infrastructure.Sitecore.Handlers
{
    using System;
    using Data;
    using Model;

    public class SupportQueryViaItemHandler : ItemRequestHandler<SitecoreQueryViaItemQuery>
    {
        private readonly IItemRepository _itemRepository;

        public SupportQueryViaItemHandler(IItemRepository itemRepository)
        {
            this._itemRepository = itemRepository;
        }

        private ItemQueryRequest GetQueryFromContenItem(Guid id, string database, string language, string version)
        {
            Item item = this._itemRepository.FindById(id, database, language, version);
            if (item == null)
            {
                throw new ArgumentException(string.Format("Query Definition ({0}) not found", id));
            }
            //ID qdId = (ID)(Type.GetType("Sitecore.Services.Infrastructure.Sitecore.Data.DataDefinitions+Templates").GetField("QueryDefinition", BindingFlags.Static | BindingFlags.Public).GetValue(null));
            if (item.TemplateID != new ID("{79E0E28F-5591-410E-A086-754EDC7CEF88}"))
            {
                throw new ArgumentException(string.Format("Item ({0}) not a Query Definition", id));
            }

            return new ItemQueryRequest
            {
                Query = item["query"],
                Database = item["database"]
            };
        }

        protected override object HandleRequest(SitecoreQueryViaItemQuery request)
        {
            ItemQueryRequest request2 = this.GetQueryFromContenItem(request.Id, request.Database, request.Language, request.Version);
            Database database = this.GetDatabase(request2.Database);
            if (request2.Query.StartsWith("fast:/"))
            {
                return database.SelectItems(request2.Query);
            }
            Item[] result = Q.SelectItems(request2.Query, database);
            if (result == null)
            {
                return new Item[0];
            }
            return result;
            //return Q.SelectItems(request2.Query, database);
        }

        internal class ItemQueryRequest
        {
            public string Database { get; set; }

            public string Query { get; set; }
        }

        private Database GetDatabase(string databaseName)
        {
            Database result;
            try
            {
                Database database = Factory.GetDatabase(this.GetDatabaseName(databaseName));
                if (database == null)
                {
                    throw new ArgumentException(this.InvalidParameterMessage("Database", databaseName));
                }
                result = database;
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(this.InvalidParameterMessage("Database", databaseName));
            }
            return result;
        }

        private string GetDatabaseName(string databaseName)
        {
            if (!string.IsNullOrEmpty(databaseName))
            {
                return databaseName.ToLower();
            }
            if (Context.ContentDatabase == null)
            {
                return Context.Database.Name;
            }
            return Context.ContentDatabase.Name;
        }

        private string InvalidParameterMessage(string input, object value)
        {
            return string.Format("{0} is invalid ({1})", input, value);
        }
    }
}