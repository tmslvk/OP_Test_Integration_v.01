namespace BPMSoft.Configuration.OPCarsBaseIntegrationJobs
{
    using BPMSoft.Configuration.Helpers;
    using BPMSoft.Configuration.OPCarsBaseIntegration.Logger;
    using BPMSoft.Configuration.Services;
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

            var userConnection =
                (UserConnection)context.Scheduler.Context.Get("UserConnection");
            var logId = Guid.Empty;

            try
            {
                var service = ClassFactory.Get<OPVehicleIntegrationHelper>(
                    new ConstructorArgument("userConnection", userConnection));

                OPCarsBaseIntegrationLogger.StartRequest(userConnection, nameof(Execute), $"OPCarsBaseIntegrationGetStockJob/{nameof(Execute)}");
                service.ImportAll();
            }
            catch (Exception ex)
            {
                OPCarsBaseIntegrationLogger.LogError(userConnection, logId, ex, true);
                throw new JobExecutionException(ex);
            }

            OPCarsBaseIntegrationLogger.CompleteResponse(userConnection, logId, $"OPCarsBaseIntegrationGetStockJob/{nameof(Execute)}", Task.CompletedTask);
            return Task.CompletedTask;
        }
    }
}