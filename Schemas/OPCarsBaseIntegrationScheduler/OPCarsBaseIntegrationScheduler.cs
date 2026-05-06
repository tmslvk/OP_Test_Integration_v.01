namespace BPMSoft.Configuration.OPCarsBaseIntegrationScheduler 
{
    using BPMSoft.Configuration.OPCarsBaseIntegrationJobs;
    using BPMSoft.Configuration.OPCronConverter;
    using BPMSoft.Core;
    using BPMSoft.Core.Scheduler;
    using Quartz;
    using System;

    public class OPCarsBaseIntegrationScheduler
    {
        private readonly UserConnection _userConnection;

        public OPCarsBaseIntegrationScheduler(UserConnection userConnection)
        {
            _userConnection = userConnection;
        }

        public void StartGetStockJob()
        {
            var cron = (string)BPMSoft.Core.Configuration.SysSettings
                .GetValue(_userConnection, "OPCarsBaseIntegrationCron");

            var jobName = "OPCarsBaseIntegrationGetStockJob";
            var jobGroup = "Main";

            var delay = (int)BPMSoft.Core.Configuration.SysSettings
                .GetValue(_userConnection, "MinutesStockRequest");

            var startTime = DateTime.UtcNow.AddMinutes(delay);

            if (AppScheduler.DoesJobExist(jobName, jobGroup))
            {
                AppScheduler.RemoveJob(jobName, jobGroup);
            }

            IJobDetail job = JobBuilder.Create<OPCarsBaseIntegrationGetStockJob>()
                .WithIdentity(jobName, jobGroup)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}Trigger", jobGroup)
                .StartAt(startTime)
                .Build();

            AppScheduler.Instance.ScheduleJob(job, trigger);
        }
    }
}