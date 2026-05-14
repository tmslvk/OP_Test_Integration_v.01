using BPMSoft.Configuration;
using BPMSoft.Configuration.Helpers;
using BPMSoft.Core;
using BPMSoft.Core.Factories;
using BPMSoft.Core.Tasks;
using System;

public class OPVehicleImportTask : IBackgroundTask<string[]>, IUserConnectionRequired
{
    protected UserConnection UserConnection;

    public void SetUserConnection(UserConnection userConnection)
    {
        UserConnection = userConnection;
    }

    public void Run(string[] param)
    {   
        var globalLock = ClassFactory.Get<OPVehicleImportGlobalLock>(
                new ConstructorArgument("userConnection", UserConnection));

        if (globalLock.IsLocked())
            return;

        globalLock.SetLocked();

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
            globalLock.SetUnlocked();
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