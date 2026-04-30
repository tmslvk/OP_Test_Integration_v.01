using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using System;
using System.Collections.Generic;

namespace BPMSoft.Configuration
{
    public abstract class OPBaseLookupService
    {
        protected readonly UserConnection UserConnection;
        protected readonly string SchemaName;

        protected string CacheKey => $"OP_LookupCache_{SchemaName}";

        protected OPBaseLookupService(UserConnection userConnection, string schemaName)
        {
            UserConnection = userConnection;
            SchemaName = schemaName;
        }

        protected Dictionary<string, Guid> GetCache()
        {
            var cache = UserConnection.ApplicationCache[CacheKey] as Dictionary<string, Guid>;
            return cache ?? new Dictionary<string, Guid>();
        }

        protected void SaveCache(Dictionary<string, Guid> cache)
        {
            UserConnection.ApplicationCache[CacheKey] = cache;
        }

        public virtual void Initialize()
        {

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, SchemaName);
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;
            var extIdCol = esq.AddColumn("OPExternalId");

            var entities = esq.GetEntityCollection(UserConnection);
            var freshCache = new Dictionary<string, Guid>();

            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);
                if (!string.IsNullOrEmpty(extId) && !freshCache.ContainsKey(extId))
                {
                    freshCache.Add(extId, entity.PrimaryColumnValue);
                }
            }

            SaveCache(freshCache);
        }

        public virtual Guid EnsureValue(DBExecutor executor, string name, string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId)) return Guid.Empty;

            var cache = GetCache();

            if (cache.TryGetValue(externalId, out Guid existingId))
            {
                return existingId;
            }

            Guid newId = Guid.NewGuid();
            var insert = new Insert(UserConnection)
                .Into(SchemaName)
                .Set("Id", Column.Parameter(newId))
                .Set("OPExternalId", Column.Parameter(externalId))
                .Set("Name", Column.Parameter(name));

            OnBeforeInsert(insert);
            insert.Execute(executor);

            cache.Add(externalId, newId);
            SaveCache(cache);

            return newId;
        }

        protected virtual void OnBeforeInsert(Insert insert) { }

        public void ClearCache()
        {
            UserConnection.ApplicationCache.Remove(CacheKey);
        }
    }

    public class OPBodyTypeService : OPBaseLookupService
    {
        private readonly OPVehicleDataProvider _dataProvider;
        public OPBodyTypeService(UserConnection userConnection) : base(userConnection, "OPVehicleBodyType") { }
     
    }

    public class OPGenerationService : OPBaseLookupService
    {
        private readonly OPVehicleDataProvider _dataProvider;
        public OPGenerationService(UserConnection userConnection) : base(userConnection, "OPVehicleGeneration") { }

    }
}