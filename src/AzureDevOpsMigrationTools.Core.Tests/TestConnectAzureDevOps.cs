using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureDevOpsMigrationTools.Core;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Linq;

namespace AzureDevOpsMigrationTools.Core.Tests
{
    [TestClass]
    public class TestConnectAzureDevOps
    {
        //[TestMethod]
        //public void TestConnect()
        //{
        //    string azureDevOpsOrganizationUrl = "https://dev.azure.com/nkdagility";
        //    //Prompt user for credential
        //    VssConnection connection = new VssConnection(new Uri(azureDevOpsOrganizationUrl), new VssClientCredentials());

        //    //create http client and query for resutls
        //    WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
        //    Wiql query = new Wiql() { Query = "SELECT [Id], [Title], [State] FROM workitems WHERE [Work Item Type] = 'Bug' AND [Assigned To] = @Me" };
        //    WorkItemQueryResult queryResults = witClient.QueryByWiqlAsync(query).Result;

        //    Assert.IsNotNull(queryResults);
        //    Assert.AreNotEqual(0, queryResults.WorkItems.Count());

        //}
    }
}
