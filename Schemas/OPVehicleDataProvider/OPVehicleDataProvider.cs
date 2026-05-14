using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using BPMSoft.Configuration.Models;


namespace BPMSoft.Configuration.Providers
{

    public class OPVehicleDataProvider
    {
        protected const string MARK_ENDPOINT = "marks";
        protected const string MODEL_ENDPOINT = "models";
        protected const string CONFIGURATION_ENDPOINT = "configurations";
        protected const string GENERATION_ENDPOINT = "generations";

        protected readonly string ApiUrl;
        protected readonly string ApiToken;

        protected readonly UserConnection UserConnection;

        public OPVehicleDataProvider(UserConnection userConnection)
        {
            UserConnection = userConnection;
            var url = Core.Configuration.SysSettings.GetValue(userConnection, "OPVehicleApiUrl", string.Empty);
            var token = Core.Configuration.SysSettings.GetValue(userConnection, "OPVehicleApiToken", string.Empty);

            ApiUrl = string.IsNullOrEmpty(url) ? "https://api.cars-base.ru" : url;
            ApiToken = string.IsNullOrEmpty(token) ? "test" : token;

        }

        public OPResult<List<VehicleBrandDto>, OPError> GetBrands()
        {
            var endpoint = $"{MARK_ENDPOINT}?";

            return GetData<VehicleBrandDto>(endpoint);
        } 

        public OPResult<List<VehicleModelDto>, OPError> GetModels()
        {
            var endpoint = $"{MODEL_ENDPOINT}?";

            return GetData<VehicleModelDto>(endpoint);
        }

        public OPResult<List<VehicleModelDto>, OPError> GetModelsByMarkId(string markId) 
        {
            var endpoint = $"{MODEL_ENDPOINT}?mark_id={markId}&";

            return GetData<VehicleModelDto>(endpoint);
        }


        public OPResult<List<VehicleConfigurationDto>, OPError> GetConfigurationByModelId(string modelId)
        {
            var endpoint = $"{CONFIGURATION_ENDPOINT}?model_id={modelId}&";

            return GetData<VehicleConfigurationDto>(endpoint);
        }

        public OPResult<List<VehicleConfigurationDto>, OPError> GetConfigurationByMarkId(string markId)
        {
            var endpoint = $"{CONFIGURATION_ENDPOINT}?mark_id={markId}&";

            return GetData<VehicleConfigurationDto>(endpoint);
        }

        public OPResult<List<VehicleGenerationDto>, OPError> GetGenerationByModelId(string modelId)
        {
            var endpoint = $"{GENERATION_ENDPOINT}?model_id={modelId}&";

            return GetData<VehicleGenerationDto>(endpoint);
        }

        public OPResult<List<VehicleGenerationDto>, OPError> GetGenerationByMarkId(string markId)
        {
            var endpoint = $"{GENERATION_ENDPOINT}?mark_id={markId}&";

            return GetData<VehicleGenerationDto>(endpoint);
        }


        public OPResult<List<TData>, OPError> GetData<TData>(string endpoint)
        {
            try
            {
                var response = GetFromApi<TData>(endpoint);

                if (response.IsFailure)
                    return response.Error;

                return response.Value.Data.ToList();
            }
            catch (Exception ex)
            {
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        protected virtual OPResult<VehicleBaseResponse<T>, OPError> GetFromApi<T>(string endpoint)
        {
            if (string.IsNullOrEmpty(ApiUrl))
                return OPErrors.API.InvalidApiToken();

            if(string.IsNullOrEmpty(ApiToken))
                return OPErrors.API.InvalidApiUrl();

            var url = $"{ApiUrl}/{endpoint}token={ApiToken}";
            Guid logId = Guid.Empty;

            try
            {
               
                logId = OPCarsBaseIntegrationLogger.StartRequest(
                    UserConnection,
                    nameof(GetFromApi),
                    url
                );

                using (var webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("user-agent", "BPMSoft-Integration-Client");
                    webClient.Headers.Add("Accept", "application/json");

                    string jsonResponse = webClient.DownloadString(url);

                    var apiResponse = JsonConvert.DeserializeObject<VehicleBaseResponse<T>>(jsonResponse);

                    OPCarsBaseIntegrationLogger.CompleteResponse(UserConnection, logId, nameof(GetFromApi), new { DataCount = apiResponse.Data.Count} );

                    return apiResponse;
                }
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(UserConnection, logId, ex, true);
                throw;
            }
        }
    }

    
}