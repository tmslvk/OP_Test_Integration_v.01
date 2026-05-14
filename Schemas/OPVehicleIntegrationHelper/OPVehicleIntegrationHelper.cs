using BPMSoft.Configuration.Validation;
using BPMSoft.Core;
using BPMSoft.Core.Factories;


namespace BPMSoft.Configuration.Helpers
{

    public class OPVehicleIntegrationHelper
    {

        protected readonly OPVehicleBrandHelper BrandService;
        protected readonly OPVehicleModelHelper ModelService;
        public OPVehicleIntegrationHelper(UserConnection userConnection)
        {
            BrandService = ClassFactory.Get<OPVehicleBrandHelper>(
                 new ConstructorArgument("userConnection", userConnection));

            ModelService = ClassFactory.Get<OPVehicleModelHelper>(
                new ConstructorArgument("userConnection", userConnection));

        }

        public OPResult<bool, OPError> ImportAll()
        {

             var response = BrandService.ImportBrands();

            if (response.IsFailure)
                return false;

            foreach(var brand in response.Value)
                ModelService.ImportModels(brand.Id, brand.ExternalId);

            return true;
        }
    }

}