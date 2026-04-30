using BPMSoft.Configuration.Validation;
using BPMSoft.Core;

namespace BPMSoft.Configuration.Services
{

    public class OPVehicleIntegrationService
    {

        private readonly OPVehicleBrandService _brandService;
        private readonly OPVehicleModelService _modelService;
        public OPVehicleIntegrationService(UserConnection userConnection)
        {
            _brandService = new OPVehicleBrandService(userConnection);
            _modelService = new OPVehicleModelService(userConnection);
        }

        public OPResult<bool, OPError> ImportAll()
        {
             var response = _brandService.ImportBrands();

            if (response.IsFailure)
                return false;

            foreach(var brand in response.Value)
                _modelService.ImportModels(brand.Id, brand.ExternalId);

            // other import logic

            return true;
        }
    }

}