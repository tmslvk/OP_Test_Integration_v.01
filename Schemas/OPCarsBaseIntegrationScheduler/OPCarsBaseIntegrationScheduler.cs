namespace BPMSoft.Configuration.OPCarsBaseIntegrationScheduler 
{
    using BPMSoft.Configuration.OPCarsBaseIntegrationJobs;
    using BPMSoft.Configuration.OPConstants;
    using BPMSoft.Configuration.OPCronConverter;
    using BPMSoft.Core;
    using BPMSoft.Core.Scheduler;
    using DocumentFormat.OpenXml.Drawing;
    using Quartz;
    using Quartz.Impl.Triggers;
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
            var jobName = "OPCarsBaseIntegrationGetStockJob";
            var jobGroup = "Main";

            var intervalRaw = (string)BPMSoft.Core.Configuration.SysSettings.GetValue(
                _userConnection,
                "OPCarsBaseIntegrationInterval"
            );

            var interval = OPSchedulerIntervals.Parse(intervalRaw);
            var startTime = DateTime.UtcNow.Add(interval);
            if (AppScheduler.DoesJobExist(jobName, jobGroup))
                AppScheduler.RemoveJob(jobName, jobGroup);

            IJobDetail job = AppScheduler.CreateProcessJob(
                jobName,
                jobGroup,
                "OPUpdateCars",
                _userConnection.Workspace.Name,
                _userConnection.CurrentUser.Name);

            ITrigger trigger = new SimpleTriggerImpl($"{jobName}Trigger", jobGroup, startTime);

            AppScheduler.Instance.ScheduleJob(job, trigger);
        }
    }
}