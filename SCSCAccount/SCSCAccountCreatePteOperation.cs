using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SalesConfigurationPlugins;
using System;
using System.ServiceModel;

namespace SCSCAccount
{
    public class SCSCAccountCreatePteOperation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Contain all the meta data
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Perform CRUD operations
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            // Logging
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    // Obtain the `SCAccount` entity from the InputParameters
                    Entity entity = (Entity)context.InputParameters["Target"];
                    // We are using early-bound, so we need to imports extra files, please take the time to create those files
                    // Verify the entity logical name match with our target entity logical name
                    if (entity.LogicalName != CRfF8_ScAccount.EntityLogicalName) return;
                    // Cast the general entity to the correspond entity of `CRfF8_ScAccount`
                    var scAccount = entity.ToEntity<CRfF8_ScAccount>();

                    // Get the values from the target entity
                    string accountNumber = scAccount.CRfF8_ScAccountNumber.ToString();
                    string accountName = scAccount.CRfF8_ScAccountName.ToString();
                    string accountStatusName = scAccount.CRfF8_ScAccountStatusName.ToString();
                    int accountStatusValue = (int)scAccount.CRfF8_ScAccountStatus.Value;
                    tracing.Trace($"accountNumber: {accountNumber} \n" +
                        $"accountName: {accountName} \n" +
                        $"statusNumber: {accountStatusValue} \n" +
                        $"statusName: {accountStatusName} \n");

                    // Hard code first record GUID
                    Guid firstRecordGUID = new Guid("61fb2530-0d1c-f011-998a-000d3aa12cf4");
                    //Guid firstRecordGUID = new Guid("61fb2530-0d1c-f011-998a-000d3aa12cf5");
                    // This doesn't return null but throw an error instead, check for error code "-2147220969"
                    Entity entityExist = service.Retrieve("crff8_scaccount", firstRecordGUID, new ColumnSet(true));

                    // Cast entity to the correspond data type
                    var entityExistCasted = entityExist.ToEntity<CRfF8_ScAccount>();
                    tracing.Trace($"Existed Entity Number: {entityExistCasted.CRfF8_ScAccountNumber}");

                    // Duplication
                    //newAccount.CRfF8_ScAccountName = "dupped yolo";
                    CRfF8_ScAccount newAccount = new CRfF8_ScAccount();
                    //newAccount = entityExistCasted.ToEntity<CRfF8_ScAccount>();
                    newAccount.CRfF8_ScAccountNumber = "yolo";
                    newAccount.CRfF8_ScAccountName = accountName + "dupped";
                    service.Create(newAccount);

                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if(ex.Detail.ErrorCode == -2147220969)
                {
                    tracing.Trace("Exception, the Retrieve() method returns null!");
                    throw new InvalidPluginExecutionException("Retrieve() method found no data!.");
                }
                tracing.Trace("FaultException Code: {0}", ex.Detail.ErrorCode.ToString());
                tracing.Trace("FaultException Message: {0}", ex.Message.ToString());
                throw new InvalidPluginExecutionException("The following error occurred in MyPlugin.", ex);
            }
            catch (Exception ex)
            {
                tracing.Trace("Exception: {0}", ex.ToString());
                throw;
            }
        }
    }
}
