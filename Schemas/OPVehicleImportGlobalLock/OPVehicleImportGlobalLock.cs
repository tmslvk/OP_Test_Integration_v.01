using BPMSoft.Core;
using System;


namespace BPMSoft.Configuration.Helpers
{

    public class OPVehicleImportGlobalLock
    {

        private readonly string _lockKey = "OPVehicleImport_GlobalLock";
        private readonly UserConnection _userConnection;
        public OPVehicleImportGlobalLock(UserConnection userConnection)
        {
            _userConnection = userConnection;
        }

        public bool IsLocked()
            =>_userConnection.ApplicationCache[_lockKey] != null;

        public void SetLocked()
            => _userConnection.ApplicationCache[_lockKey] = DateTime.Now;

        public void SetUnlocked()
            => _userConnection.ApplicationCache.Remove(_lockKey);

    }

}