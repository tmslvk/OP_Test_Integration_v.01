using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using BPMSoft.Web.Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace BPMSoft.Configuration
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OPVehicleModelService : BaseService
    {

        private Dictionary<string, DateTime> _existingData;

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]

        public OPResult<int, OPError> ImportModels(Guid brandId, string externalBrandId)
        {

            try
            {
                var dataProvider = ClassFactory.Get<OPVehicleDataProvider>(new ConstructorArgument("userConnection", UserConnection));

                var response = dataProvider.GetModelsByMarkId(externalBrandId);

                if (response.IsFailure)
                    return response.Error;

                LoadExistingData();

                using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();
                    try
                    {
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

        private void LoadExistingData()
        {
            _existingData = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "OPVehicleModel");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

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

            if (!exists)
            {
                _existingData.Add(modelDto.ExternalId, modelDto.UpdatedAt);
                InsertBrand(executor, modelDto, brandId);
            }
            else if (lastUpdate.Date != modelDto.UpdatedAt.Date)
            {
                UpdateBrand(executor, modelDto);
            }

        }

        private void InsertBrand(DBExecutor executor, VehicleModelDto dto, Guid brandId)
        {
            new Insert(UserConnection)
                 .Into("OPVehicleModel")
                 .Set("Id", Column.Parameter(Guid.NewGuid()))
                 .Set("OPName", Column.Parameter(dto.Name))
                 .Set("OPBrandId", Column.Parameter(brandId))
                 .Set("OPExternalId", Column.Parameter(dto.ExternalId))
                 .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                 .Execute(executor);
        }

        private void UpdateBrand(DBExecutor executor, VehicleModelDto dto)
        {
             new Update(UserConnection, "OPVehicleBrand")
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                .Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId))
                .Execute(executor);

        }

    }

}