using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Configuration.WUserConnectionService;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using BPMSoft.Web.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace BPMSoft.Configuration
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OPVehicleBrandService : BaseService
    {

        private Dictionary<string, DateTime> _existingBrands;

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]

        public OPResult<int, OPError> ImportBrands()
        {

            try
            {
                var dataProvider = ClassFactory.Get<OPVehicleDataProvider>(new ConstructorArgument("userConnection", UserConnection));
                var response = dataProvider.GetBrands();
                OPCarsBaseIntegrationLogger.LogResponse(UserConnection, new Guid(), response);

                if (response.IsFailure)
                    return response.Error;


                LoadExistingData();

                using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();
                    try
                    {
                        foreach (var brand in response.Value)
                        {
                            ProcessBrand(dbExecutor, brand);
                        }
                        dbExecutor.CommitTransaction();
                    }
                    catch
                    {
                        OPCarsBaseIntegrationLogger.LogError(UserConnection, new Guid(), new Exception("dbExecutor error"));
                        dbExecutor.RollbackTransaction();
                        throw;
                    }
                }
                return response.Value.Count;
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(UserConnection, new Guid(), ex);
                return OPErrors.General.Fatal(ex.Message);
            }

        }

        private void LoadExistingData()
        {
            _existingBrands = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "OPVehicleBrand");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            var entities = esq.GetEntityCollection(UserConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !_existingBrands.ContainsKey(extId))
                    _existingBrands.Add(extId, entity.GetTypedColumnValue<DateTime>(dateCol.Name));

            }
        }

        private void ProcessBrand(DBExecutor executor, VehicleBrandDto brandDto)
        {
            if (brandDto == null || string.IsNullOrEmpty(brandDto.ExternalId)) return;

            bool exists = _existingBrands.TryGetValue(brandDto.ExternalId, out DateTime lastUpdate);

            if (!exists)
            {
                _existingBrands.Add(brandDto.ExternalId, brandDto.UpdatedAt);
                InsertBrand(executor, brandDto);
            }
            else if (lastUpdate.Date != brandDto.UpdatedAt.Date)
            {
                UpdateBrand(executor, brandDto);
            }

        }

        private Guid InsertBrand(DBExecutor executor, VehicleBrandDto dto)
        {
            Guid id = Guid.NewGuid();

            new Insert(UserConnection)
                 .Into("OPVehicleBrand")
                 .Set("Id", Column.Parameter(id))
                 .Set("OPExternalId", Column.Parameter(dto.ExternalId))
                 .Set("OPExternalNumericId", Column.Parameter(dto.ExternalNumericId))
                 .Set("OPName", Column.Parameter(dto.Name))
                 .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                 .Execute(executor);

            return id;
        }

        private void UpdateBrand(DBExecutor executor, VehicleBrandDto dto)
        {
            var update = new Update(UserConnection, "OPVehicleBrand")
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                .Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId));

            update.Execute(executor);
        }

    }

}