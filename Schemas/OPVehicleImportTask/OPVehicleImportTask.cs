using BPMSoft.Configuration;
using BPMSoft.Configuration.Services;
using BPMSoft.Core;
using BPMSoft.Core.Tasks;
using Newtonsoft.Json;
using System;

public class OPVehicleImportTask : IBackgroundTask<string[]>, IUserConnectionRequired
{
    private UserConnection _userConnection;
    private const string LockKey = "OPVehicleImport_GlobalLock";

    public void SetUserConnection(UserConnection userConnection)
    {
        _userConnection = userConnection;
    }

    public void Run(string[] param)
    {

        _userConnection.ApplicationCache[LockKey] = DateTime.Now;

        try
        {
            var integrationService = new OPVehicleIntegrationService(_userConnection);
            integrationService.ImportAll();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _userConnection.ApplicationCache.Remove(LockKey);
            NotifyUser(_userConnection);
        }   
    }

    private void NotifyUser(UserConnection userConnection)
    {
        var remindingEntity = userConnection.EntitySchemaManager
            .GetInstanceByName("Reminding")
            .CreateEntity(userConnection);

        remindingEntity.SetDefColumnValues();
        remindingEntity.SetColumnValue("AuthorId", userConnection.CurrentUser.ContactId);
        remindingEntity.SetColumnValue("ContactId", userConnection.CurrentUser.ContactId);
        remindingEntity.SetColumnValue("SubjectCaption", "Импорт завершен");
        remindingEntity.SetColumnValue("Description", "Все данные об автомобилях загружены.");
        remindingEntity.SetColumnValue("RemindTime", DateTime.Now);
        remindingEntity.SetColumnValue("SysEntitySchemaId", userConnection.EntitySchemaManager.GetInstanceByName("OPVehicleBrand").UId);
        remindingEntity.Save();
    }

}