using BPMSoft.Configuration;
using BPMSoft.Configuration.Helpers;
using BPMSoft.Configuration.Services;
using BPMSoft.Configuration.WUserConnectionService;
using BPMSoft.Core;
using BPMSoft.Core.Factories;
using BPMSoft.Core.Tasks;
using Newtonsoft.Json;
using System;

public class OPVehicleImportTask : IBackgroundTask<string[]>, IUserConnectionRequired
{
    protected UserConnection UserConnection;
    private const string LockKey = "OPVehicleImport_GlobalLock";

    public void SetUserConnection(UserConnection userConnection)
    {
        UserConnection = userConnection;
    }

    public void Run(string[] param)
    {
        UserConnection.ApplicationCache[LockKey] = DateTime.Now;

        try
        {
            var integrationService = ClassFactory.Get<OPVehicleIntegrationHelper>(
                 new ConstructorArgument("userConnection", UserConnection));

            integrationService.ImportAll();
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            UserConnection.ApplicationCache.Remove(LockKey);
            NotifyUser(UserConnection);
        }   
    }

    protected virtual void NotifyUser(UserConnection userConnection)
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