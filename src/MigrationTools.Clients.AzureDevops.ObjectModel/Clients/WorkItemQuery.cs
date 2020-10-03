using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class WorkItemQuery : WorkItemQueryBase
    {

        public WorkItemQuery(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {

        }

        public override List<WorkItemData> GetWorkItems()
        {
            Log.Verbose("WorkItemQuery: ===========GetWorkItems=============");
            var wiClient = (WorkItemMigrationClient)MigrationClient.WorkItems;
            Telemetry.TrackEvent("WorkItemQuery.Execute", Parameters, null);
            Log.Verbose("WorkItemQuery: TeamProjectCollection: {QueryTarget}",  wiClient.Store.TeamProjectCollection.Uri.ToString());
            Log.Verbose("WorkItemQuery: Query: {QueryText}",  Query);
            Log.Verbose("WorkItemQuery: Paramiters: {@QueryParams}",  Parameters);

            WorkItemCollection wc;
            var startTime = DateTime.UtcNow;
            Stopwatch queryTimer = new Stopwatch();
            foreach (var item in Parameters)
            {
                Log.Verbose("WorkItemQuery: {0}: {1}", item.Key, item.Value);
            }

            queryTimer.Start();
            try
            {
                Log.Information("WorkItemQuery: Running query...");
                wc = wiClient.Store.Query(Query); //, parameters);
                queryTimer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "Query", startTime, queryTimer.Elapsed, true));
                // Add additional bits to reuse the paramiters dictionary for telemitery
                Parameters.Add("CollectionUrl", wiClient.Store.TeamProjectCollection.Uri.ToString());
                Parameters.Add("Query", Query);
                Telemetry.TrackEvent("QueryComplete",
                      Parameters,
                      new Dictionary<string, double> {
                            { "QueryTime", queryTimer.ElapsedMilliseconds },
                          { "QueryCount", wc.Count }
                      });
                Log.Information("WorkItemQuery: Query found {WorkItemCount} work items in {QueryTimer}ms ", wc.Count, queryTimer.ElapsedMilliseconds);

            }
            catch (Exception ex)
            {
                queryTimer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "Query", startTime, queryTimer.Elapsed, false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", wiClient.Store.TeamProjectCollection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "QueryTime",queryTimer.ElapsedMilliseconds }
                       });
                Log.Error(ex, " Error running query");
                throw;
            }
            return wc.ToWorkItemDataList();
        }

       
    }
}