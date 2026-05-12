using BPMSoft.Configuration.Models;
using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Providers;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Factories;
using Google.GData.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BPMSoft.Configuration.Helpers
{

    public class OPVehicleBrandHelper 
    {

        protected readonly OPVehicleDataProvider DataProvider;
        protected readonly UserConnection UserConnection;

        protected Dictionary<string, ExistingBrand> ExistingData;

        public OPVehicleBrandHelper(UserConnection userConnection)
        {
            UserConnection = userConnection;

            DataProvider = ClassFactory.Get<OPVehicleDataProvider>(
                new ConstructorArgument("userConnection", UserConnection));
        }
            
        public OPResult<List<ImportBrandDto>, OPError> ImportBrands()
        {
            try
            {  
                var response = DataProvider.GetBrands();

                if (response.IsFailure)
                    return response.Error;

                LoadExistingData(response.Value
                    .Select(b => b.ExternalId)
                    .ToArray());

                var importBrands = new List<ImportBrandDto>();

                using (DBExecutor dbExecutor = UserConnection.EnsureDBConnection())
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
                    catch (Exception)
                    {                   
                        dbExecutor.RollbackTransaction();
                        throw;
                    }
                }

                return importBrands;
            }
            catch (Exception ex)
            {
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        protected virtual void LoadExistingData(string[] externalIds)
        {
            ExistingData = new Dictionary<string, ExistingBrand>();

            var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "OPVehicleBrand");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var externalIdColumn = esq.AddColumn("OPExternalId");
            var updatedAtColumn = esq.AddColumn("OPExternalUpdatedAt");

            var filter = esq.CreateFilterWithParameters(
                FilterComparisonType.Equal,
                "OPExternalId",
                externalIds.Cast<object>());

            esq.Filters.Add(filter);

            EntityCollection entities = esq.GetEntityCollection(UserConnection);

            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(externalIdColumn.Name);

                if (!string.IsNullOrEmpty(extId) && !ExistingData.ContainsKey(extId))
                {
                    var id = entity.PrimaryColumnValue;
                    var lastUpdated = entity.GetTypedColumnValue<DateTime>(updatedAtColumn.Name);

                    ExistingData.Add(extId, new ExistingBrand() { Id = id, LastUpdated = lastUpdated });
                }

            }
        }

        protected virtual Guid ProcessBrand(DBExecutor executor, VehicleBrandDto brandDto)
        {
            if (brandDto == null || string.IsNullOrEmpty(brandDto.ExternalId)) 
                return Guid.Empty;

            bool exists = ExistingData.TryGetValue(brandDto.ExternalId, out ExistingBrand brand);

            if (!exists)
            {
                brand = new ExistingBrand
                {
                    Id = InsertBrand(executor, brandDto),
                    LastUpdated = brandDto.UpdatedAt
                };

                ExistingData.Add(brandDto.ExternalId, brand);
            }
            else if (brand.LastUpdated.Date != brandDto.UpdatedAt.Date)
            {
                UpdateBrand(executor, brandDto);
            }

            return brand.Id;
        }

        protected virtual Guid InsertBrand(DBExecutor executor, VehicleBrandDto dto)
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

        protected virtual void UpdateBrand(DBExecutor executor, VehicleBrandDto dto)
        {
            var update = new Update(UserConnection, "OPVehicleBrand")
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