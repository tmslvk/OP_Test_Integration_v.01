using BPMSoft.Configuration.Validation;
using BPMSoft.Core;

namespace BPMSoft.Configuration.Helpers
{

    public class OPVehicleIntegrationHelper
    {

        private readonly OPVehicleBrandHelper _brandService;
        private readonly OPVehicleModelHelper _modelService;
        public OPVehicleIntegrationHelper(UserConnection userConnection)
        {
            _brandService = new OPVehicleBrandHelper(userConnection);
            _modelService = new OPVehicleModelHelper(userConnection);
        }

        public OPResult<bool, OPError> ImportAll()
        {
             var response = _brandService.ImportBrands();

            if (response.IsFailure)
                return false;

            foreach(var brand in response.Value)
                _modelService.ImportModels(brand.Id, brand.ExternalId);

            return true;
        }
    }

}