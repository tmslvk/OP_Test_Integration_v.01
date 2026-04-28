using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPMSoft.Configuration.OpVehicleBrandService
{

    public class OpVehicleBrandService
    {
        private readonly UserConnection _userConnection;

        private readonly string _apiUrl;
        private readonly string _apiToken;

        private Dictionary<string, DateTime> _existingBrands;

        public OpVehicleBrandService(UserConnection userConnection)
        {
            _userConnection = userConnection;

            _apiUrl = BPMSoft.Core.Configuration.SysSettings.GetValue<string>(_userConnection, "VehicleApiUrl", string.Empty);
            _apiToken = BPMSoft.Core.Configuration.SysSettings.GetValue<string>(_userConnection, "VehicleApiToken", string.Empty);
        }

        public async Task ImportAllBrandsAndModelsAsync()
        {
            var endpoint = $"full?token={_apiToken}";
            var response = await GetFromApiAsync<VehicleBrandDto>(endpoint);

            LoadExistingData();

            foreach (var brand in response.Data)
            {
                ProcessBrand(brand); 
            }
        }

        public async Task ImportConfigurationsAsync()
        {
            var endpoint = $"configurations?token={_apiToken}";
            var response = await GetFromApiAsync<VehicleConfigurationDto>(endpoint);

            foreach (var config in response.Data)
            {
               
            }
        }

        public async Task ImportGenerationsAsync()
        {
            var endpoint = $"generations?token={_apiToken}";
            var response = await GetFromApiAsync<VehicleGenerationDto>(endpoint);

            foreach (var gen in response.Data)
            {
                
            }
        }

        
        private void LoadExistingData()
        {
            _existingBrands = new Dictionary<string, DateTime>();

            var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "OPVehicleBrand");
            esq.PrimaryQueryColumn.IsAlwaysSelect = true;

            var extIdCol = esq.AddColumn("OPExternalId");
            var dateCol = esq.AddColumn("OPExternalUpdatedAt");

            var entities = esq.GetEntityCollection(_userConnection);
            foreach (var entity in entities)
            {
                string extId = entity.GetTypedColumnValue<string>(extIdCol.Name);

                if (!string.IsNullOrEmpty(extId) && !_existingBrands.ContainsKey(extId))
                    _existingBrands.Add(extId, entity.GetTypedColumnValue<DateTime>(dateCol.Name));

            }
        }

        private void ProcessBrand(VehicleBrandDto brandDto)
        {
            if (brandDto == null || string.IsNullOrEmpty(brandDto.ExternalId))
                return;
         
            bool exists = _existingBrands.TryGetValue(brandDto.ExternalId, out DateTime lastUpdate);

            if (!exists)
                InsertBrand(brandDto);
            else if (lastUpdate != brandDto.UpdatedAt)
                UpdateBrand(brandDto);

        }

        private void InsertBrand(VehicleBrandDto dto)
        {
            var insert = new Insert(_userConnection)
                .Into("OPVehicleBrand")
                .Set("OPExternalId", Column.Parameter(dto.ExternalId))
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPCountry", Column.Parameter(dto.Country))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt));
            insert.Execute();
        }

        private void UpdateBrand(VehicleBrandDto dto)
        {
            var update = new Update(_userConnection, "OpVehicleBrand")
                .Set("OpName", Column.Parameter(dto.Name))
                .Set("OpCountry", Column.Parameter(dto.Country))
                .Set("OpExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                .Where("OpExternalId").IsEqual(Column.Parameter(dto.ExternalId));
            update.Execute();
        }

        private async Task<CarsBaseResponse<T>> GetFromApiAsync<T>(string endpoint)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiToken))
            {
                throw new Exception("Ошибка интеграции: Не заполнены системные настройки [cite: 19, 60]");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiUrl);
                try
                {
                    var responseMessage = await client.GetAsync(endpoint);
                    responseMessage.EnsureSuccessStatusCode();

                    var content = await responseMessage.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CarsBaseResponse<T>>(content);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }

    public class CarsBaseResponse<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }

    public class VehicleBrandDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("models")]
        public List<VehicleModelDto> Models { get; set; }
    }

    public class VehicleModelDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year_from")]
        public int YearFrom { get; set; } 

        [JsonProperty("year_to")]
        public int? YearTo { get; set; }

        [JsonProperty("class")]
        public string VehicleClass { get; set; } 

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class VehicleConfigurationDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("model_id")]
        public string ModelExternalId { get; set; }

        [JsonProperty("body_type")]
        public string BodyType { get; set; }  

        [JsonProperty("doors_count")]
        public int DoorsCount { get; set; } 

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class VehicleGenerationDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("model_id")]
        public string ModelExternalId { get; set; }

        [JsonProperty("name")]
        public string BodyType { get; set; }

        [JsonProperty("year_from")]
        public DateTime YearFrom { get; set; }

        [JsonProperty("year_to")]
        public DateTime YearTo { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }
}