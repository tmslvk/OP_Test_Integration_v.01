using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using BPMSoft.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace BPMSoft.Configuration
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OPVehicleModelService : BaseService
    {
        private OPVehicleDataProvider _dataProvider;
        private Dictionary<string, DateTime> _existingData;
        private OPBodyTypeService _bodyTypeService;
        private OPGenerationService _generationService;

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]

        public OPResult<int, OPError> ImportModels(Guid brandId, string externalBrandId)
        {

            try
            {
                _dataProvider = ClassFactory.Get<OPVehicleDataProvider>(new ConstructorArgument("userConnection", UserConnection));

                var response = _dataProvider.GetModelsByMarkId(externalBrandId);

                if (response.IsFailure)
                    return response.Error;

                LoadExistingData(brandId);

                using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();
                    try
                    {
                        _bodyTypeService = new OPBodyTypeService(UserConnection, _dataProvider);
                        _generationService = new OPGenerationService(UserConnection, _dataProvider);

                        _bodyTypeService.Initialize();
                        _generationService.Initialize();

                        foreach (var model in response.Value)
                        {
                            ProcessModel(dbExecutor, model, brandId);
                        }
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

        private void LoadExistingData(Guid brandId)
        {
            _existingData = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "OPVehicleModel");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "OPBrand", brandId));

            var entities = esq.GetEntityCollection(UserConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !_existingData.ContainsKey(extId))
                    _existingData.Add(extId, entity.GetTypedColumnValue<DateTime>(dateCol.Name));

            }
        }

        private void ProcessModel(DBExecutor executor, VehicleModelDto modelDto, Guid brandId)
        {
            if (modelDto == null || string.IsNullOrEmpty(modelDto.ExternalId)) return;

            bool exists = _existingData.TryGetValue(modelDto.ExternalId, out DateTime lastUpdate);

            var configResponse = _bodyTypeService.GetConfigurationByModelId(modelDto.ExternalId);
            var generationResponse = _generationService.GetGenerationByModelId(modelDto.ExternalId);

            Guid bodyTypeId = Guid.Empty;
            Guid generationId = Guid.Empty;

            if (configResponse.IsSuccess && configResponse.Value != null)
            {
                var config = configResponse.Value.FirstOrDefault();
                bodyTypeId = _bodyTypeService.EnsureValue(executor, config.BodyType, config.ExternalId);
            }

            if (generationResponse.IsSuccess && generationResponse.Value != null)
            {
                var config = generationResponse.Value.FirstOrDefault();

                if (config.BodyType == null)
                    config.BodyType = $"{config.YearFrom}-{config.YearTo}";

                generationId = _generationService.EnsureValue(executor, config.BodyType, config.ExternalId);
            }

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
     
        private void InsertModel(DBExecutor executor, VehicleModelDto dto, Guid brandId, Guid bodyTypeId, Guid generationId)
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
        
        private void UpdateModel(DBExecutor executor, VehicleModelDto dto, Guid bodyTypeId, Guid generationId)
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

    }

}