using BPMSoft.Core;
using BPMSoft.Core.Entities;
using BPMSoft.Core.Entities.Events;
using BPMSoft.Core.Configuration;
using System;
using System.Collections.Generic;

namespace BPMSoft.Configuration
{

    [EntityEventListener(SchemaName = "SysSettingsValue")]
    public class OPSysSettingsChangedListener : BaseEntityEventListener
    {
        public override void OnSaved(object sender, EntityAfterEventArgs e)
        {
            base.OnSaved(sender, e);
            var entity = (Entity)sender;
            var userConnection = entity.UserConnection;

            Guid settingId = entity.GetTypedColumnValue<Guid>("SysSettingsId");

            var schema = userConnection.EntitySchemaManager.GetInstanceByName("SysSettings");

            var settingEntity = schema.CreateEntity(userConnection);
            if (settingEntity.FetchFromDB(settingId) && settingEntity.GetTypedColumnValue<string>("Code") == "OPSchedulerRepeatIntervalInDays")
            {
                RunProcess(userConnection);
            }
        }

        private void RunProcess(UserConnection userConnection)
        {
            string processName = "OPRequestProcessingProcess";

            var processEngine = userConnection.ProcessEngine;
            processEngine.ProcessExecutor.Execute(processName);
        }
    }
}