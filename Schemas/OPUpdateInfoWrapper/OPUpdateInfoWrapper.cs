namespace BPMSoft.Configuration.OPCarsBaseIntegrationJobs
{
    using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
    using BPMSoft.Configuration.OPVehicleBrandService;
    using BPMSoft.Configuration.WUserConnectionService;
    using BPMSoft.Core;
    using BPMSoft.Core.Factories;
    using Quartz;
    using System;
    using System.Threading.Tasks;

    public class OPCarsBaseIntegrationGetStockJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {

            var userConnection = ClassFactory.Get<UserConnection>();

            var logId = OPCarsBaseIntegrationLogger.StartRequest(
                userConnection,
                nameof(Execute),
                $"OPCarsBaseIntegrationGetStockJob/{nameof(Execute)}"
            );

            try
            {
                var service = ClassFactory.Get<OPVehicleBrandService>(
                    new ConstructorArgument("userConnection", userConnection));

                OPCarsBaseIntegrationLogger.StartRequest(userConnection, nameof(Execute), $"OPCarsBaseIntegrationGetStockJob/{nameof(Execute)}");
                service.ImportBrands();
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(userConnection, logId, ex, true);
                throw new JobExecutionException(ex);
            }

            OPCarsBaseIntegrationLogger.CompleteResponse(userConnection, logId, $"OPCarsBaseIntegrationGetStockJob/{nameof(Execute)}", "JOB_START_"+ Task.CompletedTask);
            return Task.CompletedTask;
        }
    }
}