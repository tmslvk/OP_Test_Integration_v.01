using BPMSoft.Configuration.Helpers;
using BPMSoft.Configuration.Models;
using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BPMSoft.Configuration
{
    public class OPVehicleModelHelper
    {
        protected OPVehicleDataProvider DataProvider;
        protected Dictionary<string, DateTime> ExistingData;
        protected OPBodyTypeHelper BodyTypeHelper;
        protected OPGenerationHelper GenerationHelper;
        protected UserConnection UserConnection;

        public OPVehicleModelHelper(UserConnection userConnection)
            => UserConnection = userConnection;

        public OPResult<int, OPError> ImportModels(Guid brandId, string externalBrandId)
        {
            try
            {
                DataProvider = ClassFactory.Get<OPVehicleDataProvider>(
                    new ConstructorArgument("userConnection", UserConnection));

                var response = DataProvider.GetModelsByMarkId(externalBrandId);
                var configsResponse = DataProvider.GetConfigurationByMarkId(externalBrandId);
                var generationsResponse = DataProvider.GetGenerationByMarkId(externalBrandId);

                if (response.IsFailure)
                    return response.Error;

                var configMap = (configsResponse.Value ?? new List<VehicleConfigurationDto>())
                    .ToLookup(c => c.ModelExternalId);

                var generationMap = (generationsResponse.Value ?? new List<VehicleGenerationDto>())
                    .ToLookup(g => g.ModelExternalId);


                LoadExistingData(brandId);

                using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();
                    try
                    {
                        BodyTypeHelper = ClassFactory.Get<OPBodyTypeHelper>(
                            new ConstructorArgument("userConnection", UserConnection));
                        GenerationHelper = ClassFactory.Get<OPGenerationHelper>(
                            new ConstructorArgument("userConnection", UserConnection));

                        BodyTypeHelper.Initialize();
                        GenerationHelper.Initialize();

                        foreach (var model in response.Value)
                        {
                            var configuration = configMap[model.ExternalId].FirstOrDefault();
                            var generation = generationMap[model.ExternalId].FirstOrDefault();

                            ProcessModel(dbExecutor, model, brandId, configuration, generation);
                        }

                        UpdateBrandLoadedStatus(dbExecutor, brandId);

                        dbExecutor.CommitTransaction();
                    }
                    catch
                    {
                        dbExecutor.RollbackTransaction();
                        throw;
                    }
                }
                return response.Value.Count;
            }
            catch (Exception ex)
            {
                return OPErrors.General.Fatal(ex.Message);
            }

        }

        protected virtual void ProcessModel(DBExecutor executor, VehicleModelDto modelDto, Guid brandId, VehicleConfigurationDto configuration, VehicleGenerationDto generation)
        {
            if (modelDto == null || string.IsNullOrEmpty(modelDto.ExternalId)) return;

            bool exists = ExistingData.TryGetValue(modelDto.ExternalId, out DateTime lastUpdate);

            Guid bodyTypeId = GetBodyTypeId(executor, configuration);
            Guid generationId = GetGenerationId(executor, generation);

            if (!exists)
            {
                ExistingData.Add(modelDto.ExternalId, modelDto.UpdatedAt);
                InsertModel(executor, modelDto, brandId, bodyTypeId, generationId);
            }
            else if (lastUpdate.Date != modelDto.UpdatedAt.Date)
            {
                UpdateModel(executor, modelDto, bodyTypeId, generationId);
            }

        }

        protected virtual Guid GetBodyTypeId(DBExecutor executor, VehicleConfigurationDto configuration)
        {
            if(configuration == null)
                return Guid.Empty;

            return BodyTypeHelper.EnsureValue(executor, configuration.BodyType, configuration.ExternalId);
        }

        protected virtual Guid GetGenerationId(DBExecutor executor, VehicleGenerationDto generation)
        {
            if(generation == null)
                return Guid.Empty;

            if (generation.BodyType == null)
                generation.BodyType = $"{generation.YearFrom}-{generation.YearTo}";

            return GenerationHelper.EnsureValue(executor, generation.BodyType, generation.ExternalId);
        }

        protected virtual void InsertModel(DBExecutor executor, VehicleModelDto dto, Guid brandId, Guid bodyTypeId, Guid generationId)
        {
            var insert = new Insert(UserConnection)
                 .Into("OPVehicleModel")
                 .Set("Id", Column.Parameter(Guid.NewGuid()))
                 .Set("OPName", Column.Parameter(dto.Name))
                 .Set("OPBrandId", Column.Parameter(brandId))
                 .Set("OPExternalId", Column.Parameter(dto.ExternalId))
                 .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt));

            if (bodyTypeId != Guid.Empty)
                insert.Set("OPBodyTypeId", Column.Parameter(bodyTypeId));

            if (generationId != Guid.Empty) 
                insert.Set("OPGenerationId", Column.Parameter(generationId));

            insert.Execute(executor);
        }

        protected virtual void UpdateModel(DBExecutor executor, VehicleModelDto dto, Guid bodyTypeId, Guid generationId)
        {
            var update = new Update(UserConnection, "OPVehicleModel")
               .Set("OPName", Column.Parameter(dto.Name))
               .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt));          

            if (bodyTypeId != Guid.Empty)
                update.Set("OPBodyTypeId", Column.Parameter(bodyTypeId));
            if (generationId != Guid.Empty)
                update.Set("OPGenerationId", Column.Parameter(generationId));

            update.Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId));

            update.Execute(executor);

        }

        protected virtual void UpdateBrandLoadedStatus(DBExecutor executor, Guid brandId)
        {
            new Update(UserConnection, "OPVehicleBrand")
                .Set("OPIsModelsLoaded", Column.Parameter(true))
                .Where("Id").IsEqual(Column.Parameter(brandId))
                .Execute(executor);
        }

        protected virtual void LoadExistingData(Guid brandId)
        {
            ExistingData = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "OPVehicleModel");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "OPBrand", brandId));

            var entities = esq.GetEntityCollection(UserConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !ExistingData.ContainsKey(extId))
                    ExistingData.Add(extId, entity.GetTypedColumnValue<DateTime>(dateCol.Name));

            }
        }

    }

}