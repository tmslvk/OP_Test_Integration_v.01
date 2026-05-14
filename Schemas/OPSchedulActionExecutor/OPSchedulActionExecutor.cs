using BPMSoft.Configuration.OPCommonLogger;
using BPMSoft.Core;
using BPMSoft.Core.Tasks;
using System;
using CommonLogger = BPMSoft.Configuration.OPCommonLogger.OPCommonLogger;

namespace BPMSoft.Configuration.Services
{

    public class OPSchedulActionExecutor
    {
        public void Execute(UserConnection userConnection)
        {
            CommonLogger.WriteInformationLog(userConnection, nameof(Execute), "Запущен полный импорт данных (планировщик)");
            var param = Array.Empty<string>();
            Task.StartNewWithUserConnection<OPVehicleImportTask, string[]>(param);

        }
    }
}