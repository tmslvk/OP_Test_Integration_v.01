using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using System;
using System.Collections.Generic;

namespace BPMSoft.Configuration.Services
{

    public class OPVehicleBrandService 
    {

        private readonly OPVehicleDataProvider _dataProvider;
        private readonly UserConnection _userConnection;

        private Dictionary<string, ExistingBrand> _existingData;

        public OPVehicleBrandService(UserConnection userConnection)
        {
            _userConnection = userConnection;

            _dataProvider = ClassFactory.Get<OPVehicleDataProvider>(
                new ConstructorArgument("userConnection", _userConnection));
        }
            
        public OPResult<List<ImportBrandDto>, OPError> ImportBrands()
        {

            Guid logId = Guid.Empty;

            try
            {
      
                logId = OPCarsBaseIntegrationLogger.StartRequest(
                    _userConnection,
                    nameof(ImportBrands),
                    $"OPVehicleBrand"
                );

                var response = _dataProvider.GetBrands();

                if (response.IsFailure)
                    return response.Error;

                LoadExistingData();

                var importBrands = new List<ImportBrandDto>();

                using (DBExecutor dbExecutor = _userConnection.EnsureDBConnection())
                {
                    dbExecutor.StartTransaction();

                    try
                    {
                        foreach (var brand in response.Value)
                        {
                            var id = ProcessBrand(dbExecutor, brand);

                            importBrands.Add(new ImportBrandDto() { Id = id, ExternalId = brand.ExternalId });
                        }

                        dbExecutor.CommitTransaction();
                    }
                    catch (Exception dbEx)
                    {
                        OPCarsBaseIntegrationLogger.LogError(_userConnection, logId, dbEx, true);
                        dbExecutor.RollbackTransaction();
                        throw;
                    }
                }

                OPCarsBaseIntegrationLogger.CompleteResponse(_userConnection, logId, nameof(ImportBrands), response.Value.Count);

                return importBrands;
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(_userConnection, logId, ex, true);
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        private void LoadExistingData()
        {
            _existingData = new Dictionary<string, ExistingBrand>();

            var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "OPVehicleBrand");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            var entities = esq.GetEntityCollection(_userConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !_existingData.ContainsKey(extId))
                {
                    var id = entity.PrimaryColumnValue;
                    var lastUpdated = entity.GetTypedColumnValue<DateTime>(dateCol.Name);

                    _existingData.Add(extId, new ExistingBrand() { Id = id, LastUpdated = lastUpdated });
                }

            }
        }

        private Guid ProcessBrand(DBExecutor executor, VehicleBrandDto brandDto)
        {
            if (brandDto == null || string.IsNullOrEmpty(brandDto.ExternalId)) 
                return Guid.Empty;

            bool exists = _existingData.TryGetValue(brandDto.ExternalId, out ExistingBrand brand);

            if (!exists)
            {
                brand = new ExistingBrand();
                brand.Id = InsertBrand(executor, brandDto);
                brand.LastUpdated = brand.LastUpdated;

                _existingData.Add(brandDto.ExternalId, brand);
            }
            else if (brand.LastUpdated.Date != brandDto.UpdatedAt.Date)
            {
                UpdateBrand(executor, brandDto);
            }

            return brand.Id;
        }

        private Guid InsertBrand(DBExecutor executor, VehicleBrandDto dto)
        {
            Guid id = Guid.NewGuid();

            new Insert(_userConnection)
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
            var update = new Update(_userConnection, "OPVehicleBrand")
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                .Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId));

            update.Execute(executor);
        }

    }

    public class ImportBrandDto
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; }
    }

    public class ExistingBrand
    {
        public Guid Id { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}