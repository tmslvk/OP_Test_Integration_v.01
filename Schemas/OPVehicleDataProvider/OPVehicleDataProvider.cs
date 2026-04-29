using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Web.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using System.Text;

namespace BPMSoft.Configuration.Providers
{

    public class OPVehicleDataProvider
    {
        private const string MARK_ENDPOINT = "marks";

        private readonly string _apiUrl;
        private readonly string _apiToken;
        private readonly UserConnection _userConnection;
        private string MarkEndPoint => $"{MARK_ENDPOINT}?token={_apiToken}";

        public OPVehicleDataProvider(UserConnection userConnection)
        {
            this._userConnection = userConnection;
            var url = Core.Configuration.SysSettings.GetValue(userConnection, "VehicleApiUrl", string.Empty);
            var token = Core.Configuration.SysSettings.GetValue(userConnection, "VehicleApiToken", string.Empty);

            _apiUrl = string.IsNullOrEmpty(url) ? "https://api.cars-base.ru" : url;
            _apiToken = string.IsNullOrEmpty(token) ? "test" : token;

        }

        public OPResult<List<VehicleBrandDto>, OPError> GetBrands()
        {
            Guid logId = Guid.Empty;

            try
            {
                var url = $"{_apiUrl}/{MarkEndPoint}";

                logId = OPCarsBaseIntegrationLogger.StartRequest(
                    _userConnection,
                    nameof(GetBrands),
                    url
                );

                var response = GetFromApi<VehicleBrandDto>(MarkEndPoint, logId);

                if (response.IsFailure)
                    return response.Error;

                return response.Value.Data.ToList();
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(_userConnection, logId, ex, true);
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        private OPResult<CarsBaseResponse<T>, OPError> GetFromApi<T>(string endpoint, Guid logId)
        {
            var url = $"{_apiUrl}/{endpoint}";

            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("user-agent", "BPMSoft-Integration-Client");
                    webClient.Headers.Add("Accept", "application/json");

                    string jsonResponse = webClient.DownloadString(url);

                    var apiResponse = JsonConvert.DeserializeObject<CarsBaseResponse<T>>(jsonResponse);
                    OPCarsBaseIntegrationLogger.CompleteResponse(_userConnection, logId, nameof(GetFromApi), apiResponse);
                    return apiResponse;
                }
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(_userConnection, logId, ex, true);
                throw;
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