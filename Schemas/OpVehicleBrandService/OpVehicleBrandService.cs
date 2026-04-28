using BPMSoft.Core;
using BPMSoft.Core.DB;
using BPMSoft.Core.Entities;
using BPMSoft.Web.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace BPMSoft.Configuration
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OPVehicleBrandService : BaseService
    {
        private string ApiUrl => "https://api.cars-base.ru/";
        private string ApiToken => "test";

        private UserConnection _userConnection;
        private Dictionary<string, DateTime> _existingBrands;


        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]
        public string ImportAllBrandsAndModels()
        {
            _userConnection = UserConnection;

            var testData = Core.Configuration.SysSettings.GetValue<string>(UserConnection, "VehicleApiToken", string.Empty);

            var endpoint = $"full?token={ApiToken}";
            var response = GetFromApi<VehicleBrandDto>(endpoint);

            LoadExistingData();

            foreach (var brand in response.Data)
            {
                ProcessBrand(brand); 
            }

            return "OK";
        }

        public void ImportConfigurations()
        {
            var endpoint = $"configurations?token={ApiToken}";
            var response = GetFromApi<VehicleConfigurationDto>(endpoint);

            foreach (var config in response.Data)
            {
               
            }
        }

        public void ImportGenerations()
        {
            var endpoint = $"generations?token={ApiToken}";
            var response = GetFromApi<VehicleGenerationDto>(endpoint);

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
                .Set("OPExternalNumericId", Column.Parameter(dto.ExternalNumericId))
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt));

            insert.Execute();
        }

        private void UpdateBrand(VehicleBrandDto dto)
        {
            var update = new Update(_userConnection, "OpVehicleBrand")
                .Set("OPName", Column.Parameter(dto.Name))
                .Set("OPCountry", Column.Parameter(dto.Country))
                .Set("OPExternalUpdatedAt", Column.Parameter(dto.UpdatedAt))
                .Where("OPExternalId").IsEqual(Column.Parameter(dto.ExternalId));

            update.Execute();
        }

        private CarsBaseResponse<T> GetFromApi<T>(string endpoint)
        {
            if (string.IsNullOrEmpty(ApiUrl) || string.IsNullOrEmpty(ApiToken))
            {
                throw new Exception("Ошибка интеграции: Не заполнены системные настройки");
            }

            var url = ApiUrl + endpoint;

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("user-agent", "BPMSoft-Integration-Client");
                webClient.Headers.Add("Accept", "application/json");


                string jsonResponse = webClient.DownloadString(url);

                var apiResponse = JsonConvert.DeserializeObject<CarsBaseResponse<T>>(jsonResponse);

                return apiResponse;
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

        [JsonProperty("numeric_id")]
        public string ExternalNumericId { get; set; }

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