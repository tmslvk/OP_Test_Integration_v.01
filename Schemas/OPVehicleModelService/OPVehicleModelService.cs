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

    public class OPVehicleModelService
    {
        private OPVehicleDataProvider _dataProvider;
        private Dictionary<string, DateTime> _existingData;
        private OPBodyTypeService _bodyTypeService;
        private OPGenerationService _generationService;
        private UserConnection _userConnection;


        public OPVehicleModelService(UserConnection userConnection)
            => _userConnection = userConnection;

        public OPResult<int, OPError> ImportModels(Guid brandId, string externalBrandId)
        {

            try
            {
                _dataProvider = ClassFactory.Get<OPVehicleDataProvider>(new ConstructorArgument("userConnection", _userConnection));

                var response = _dataProvider.GetModelsByMarkId(externalBrandId);
                var configsResponse = _dataProvider.GetConfigurationByMarkId(externalBrandId);
                var generationsResponse = _dataProvider.GetGenerationByMarkId(externalBrandId);

                if (response.IsFailure)
                    return response.Error;

                var configMap = (configsResponse.Value ?? new List<VehicleConfigurationDto>())
                    .ToLookup(c => c.ModelExternalId);

                var generationMap = (generationsResponse.Value ?? new List<VehicleGenerationDto>())
                    .ToLookup(g => g.ModelExternalId);


                LoadExistingData(brandId);

                using (DBExecutor dbExecutor = _userConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();
                    try
                    {
                        _bodyTypeService = new OPBodyTypeService(_userConnection);
                        _generationService = new OPGenerationService(_userConnection);

                        _bodyTypeService.Initialize();
                        _generationService.Initialize();

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

        private void ProcessModel(DBExecutor executor, VehicleModelDto modelDto, Guid brandId, VehicleConfigurationDto configuration, VehicleGenerationDto generation)
        {
            if (modelDto == null || string.IsNullOrEmpty(modelDto.ExternalId)) return;

            bool exists = _existingData.TryGetValue(modelDto.ExternalId, out DateTime lastUpdate);

            Guid bodyTypeId = GetBodyTypeId(executor, configuration);
            Guid generationId = GetGenerationId(executor, generation);

            if (!exists)
            {
                _existingData.Add(modelDto.ExternalId, modelDto.UpdatedAt);
                InsertModel(executor, modelDto, brandId, bodyTypeId, generationId);
            }
            else if (lastUpdate.Date != modelDto.UpdatedAt.Date)
            {
                UpdateModel(executor, modelDto, bodyTypeId, generationId);
            }

        }

        private Guid GetBodyTypeId(DBExecutor executor, VehicleConfigurationDto configuration)
        {
            if(configuration == null)
                return Guid.Empty;

            return _bodyTypeService.EnsureValue(executor, configuration.BodyType, configuration.ExternalId);
        }

        private Guid GetGenerationId(DBExecutor executor, VehicleGenerationDto generation)
        {
            if(generation == null)
                return Guid.Empty;

            if (generation.BodyType == null)
                generation.BodyType = $"{generation.YearFrom}-{generation.YearTo}";

            return _generationService.EnsureValue(executor, generation.BodyType, generation.ExternalId);
        }

        private void InsertModel(DBExecutor executor, VehicleModelDto dto, Guid brandId, Guid bodyTypeId, Guid generationId)
        {
            var insert = new Insert(_userConnection)
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
        
        private void UpdateModel(DBExecutor executor, VehicleModelDto dto, Guid bodyTypeId, Guid generationId)
        {
            var update = new Update(_userConnection, "OPVehicleModel")
               .Set("OPName", Column.Parameter(dto.Name))
               .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt));          

            if (bodyTypeId != Guid.Empty)
                update.Set("OPBodyTypeId", Column.Parameter(bodyTypeId));
            if (generationId != Guid.Empty)
                update.Set("OPGenerationId", Column.Parameter(generationId));

            update.Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId));

            update.Execute(executor);

        }

        private void UpdateBrandLoadedStatus(DBExecutor executor, Guid brandId)
        {
            new Update(_userConnection, "OPVehicleBrand")
                .Set("OPIsModelsLoaded", Column.Parameter(true))
                .Where("Id").IsEqual(Column.Parameter(brandId))
                .Execute(executor);
        }

        private void LoadExistingData(Guid brandId)
        {
            _existingData = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "OPVehicleModel");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "OPBrand", brandId));

            var entities = esq.GetEntityCollection(_userConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !_existingData.ContainsKey(extId))
                    _existingData.Add(extId, entity.GetTypedColumnValue<DateTime>(dateCol.Name));

            }
        }

    }

}