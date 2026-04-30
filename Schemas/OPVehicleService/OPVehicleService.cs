using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
using BPMSoft.Configuration.Services;
using BPMSoft.Configuration.Validation;
using BPMSoft.Core.Tasks;
using BPMSoft.Web.Common;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace BPMSoft.Configuration.OPVehicleBrandService
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OPVehicleService : BaseService
    {

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]
        public OPResult<int, OPError> ImportBrands()
        {
            Guid logId = Guid.Empty;

            try
            {
                var requestContext = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.LocalPath.ToString();

                logId = OPCarsBaseIntegrationLogger.StartRequest(
                    UserConnection,
                    nameof(ImportBrands),
                    $"{requestContext}"
                );

                Services.OPVehicleBrandService service = new Services.OPVehicleBrandService(base.UserConnection);

                var response = service.ImportBrands();

                if (response.IsFailure)
                    return response.Error;


                OPCarsBaseIntegrationLogger.CompleteResponse(UserConnection, logId, nameof(ImportBrands), response.Value);

                return response.Value.Count;
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(UserConnection, logId, ex, true);
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]
        public OPResult<int, OPError> ImportModels(Guid brandId, string externalBrandId)
        {
            Guid logId = Guid.Empty;

            try
            {

                var requestContext = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.LocalPath.ToString();

                logId = OPCarsBaseIntegrationLogger.StartRequest(
                    UserConnection,
                    nameof(ImportModels),
                    $"{requestContext}"
                );

                OPVehicleModelService service = new OPVehicleModelService(base.UserConnection);

                var response = service.ImportModels(brandId, externalBrandId);

                if (response.IsFailure)
                    return response.Error;


                OPCarsBaseIntegrationLogger.CompleteResponse(UserConnection, logId, nameof(ImportModels), response.Value);

                return response.Value;
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(UserConnection, logId, ex, true);
                return OPErrors.General.Fatal(ex.Message);
            }
        }

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json)]
        public OPResult<string, OPError> StartFullImportTask()
        {
            var param = Array.Empty<string>();

            string lockKey = "OPVehicleImport_GlobalLock";

            var activeLock = UserConnection.ApplicationCache[lockKey];

            if (activeLock != null)
                return "Импорт уже запущен другим пользователем или системой. Дождитесь завершения.";


            Task.StartNewWithUserConnection<OPVehicleImportTask, string[]>(param);

            return "Задача запущена";
        }

    }


}

