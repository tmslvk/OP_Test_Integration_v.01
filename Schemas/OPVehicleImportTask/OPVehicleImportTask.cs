using BPMSoft.Configuration;
using BPMSoft.Configuration.Helpers;
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
            var integrationService = new OPVehicleIntegrationHelper(_userConnection);
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
        RemindingUtilities reminding = new RemindingUtilities();

        var id = userConnection.EntitySchemaManager.GetItemByName("OPVehicleBrand").UId;
        var currentContactId = userConnection.CurrentUser.ContactId;

        reminding.CreateReminding(userConnection, new RemindingConfig(id)
        {
            AuthorId = currentContactId,
            ContactId = currentContactId,
            Description = "Все данные об автомобилях загружены.",
            RemindTime = DateTime.Now,
            IsNeedToSend = true
        });
    }

}