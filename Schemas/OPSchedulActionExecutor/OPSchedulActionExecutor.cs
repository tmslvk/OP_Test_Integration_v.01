using BPMSoft.Core;
using BPMSoft.Core.Tasks;
using System;


namespace BPMSoft.Configuration.Services
{

    public class OPSchedulActionExecutor
    {
        public void Execute(UserConnection userConnection)
        {
            var param = Array.Empty<string>();
            Task.StartNewWithUserConnection<OPVehicleImportTask, string[]>(param);

        }
    }
}