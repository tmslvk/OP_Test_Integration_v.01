using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Reporting.DataSource.Abstractions;
using System;
using System.Collections.Generic;

namespace BPMSoft.Configuration
{
    public abstract class OPBaseLookupService
    {
        protected readonly UserConnection UserConnection;
        protected readonly string SchemaName;
        protected Dictionary<string, Guid> Cache;

        protected OPBaseLookupService(UserConnection userConnection, string schemaName)
        {
            UserConnection = userConnection;
            SchemaName = schemaName;
            Cache = new Dictionary<string, Guid>();
        }

        public virtual void Initialize()
        {
            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, SchemaName);
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;
            var extIdCol = esq.AddColumn("OPExternalId");

            var entities = esq.GetEntityCollection(UserConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);
                if (!string.IsNullOrEmpty(extId) && !Cache.ContainsKey(extId))
                {
                    Cache.Add(extId, entity.PrimaryColumnValue);
                }
            }
        }

        public virtual Guid EnsureValue(DBExecutor executor, string name, string externalId)
        {
            if (string.IsNullOrWhiteSpace(externalId)) return Guid.Empty;

            if (Cache.TryGetValue(externalId, out Guid existingId))
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

            Cache.Add(externalId, newId);
            return newId;
        }

        protected virtual void OnBeforeInsert(Insert insert) { }
    }

    public class OPBodyTypeService : OPBaseLookupService
    {
        private readonly OPVehicleDataProvider _dataProvider;
        public OPBodyTypeService(UserConnection userConnection, OPVehicleDataProvider dataProvider)
            : base(userConnection, "OPVehicleBodyType")
            => _dataProvider = dataProvider;

        public OPResult<List<VehicleConfigurationDto>, OPError> GetConfigurationByModelId(string modelId)
            => _dataProvider.GetConfigurationByModelId(modelId);
        
    }

    public class OPGenerationService : OPBaseLookupService
    {
        private readonly OPVehicleDataProvider _dataProvider;
        public OPGenerationService(UserConnection userConnection, OPVehicleDataProvider dataProvider)
            : base(userConnection, "OPVehicleGeneration")
            => _dataProvider = dataProvider;


        public OPResult<List<VehicleGenerationDto>, OPError> GetGenerationByModelId(string modelId)
            => _dataProvider.GetGenerationByModelId(modelId);
    }
}