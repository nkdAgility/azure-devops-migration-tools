using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsWorkItemQuery : WorkItemQueryBase
    {
        public TfsWorkItemQuery(ITelemetryLogger telemetry)
            : base(telemetry)
        {
        }

        new TfsTeamProjectEndpoint MigrationClient => (TfsTeamProjectEndpoint)base.MigrationClient;

        public override List<int> GetWorkItemIds()
        {
            return GetInternalWorkItems().Select(wi => wi.Id).ToList();
        }

        public override List<WorkItemData> GetWorkItems()
        {
            return GetInternalWorkItems().ToWorkItemDataList();
        }

        private IList<WorkItem> GetInternalWorkItems()
        {
            Log.Debug("WorkItemQuery: ===========GetWorkItems=============");
            var wiClient = (TfsWorkItemMigrationClient)MigrationClient.WorkItems;
            Telemetry.TrackEvent("WorkItemQuery.Execute", Parameters, null);
            Log.Debug("WorkItemQuery: TeamProjectCollection: {QueryTarget}", wiClient.Store.TeamProjectCollection.Uri.ToString());
            Log.Debug("WorkItemQuery: Query: {QueryText}", Query);
            Log.Debug("WorkItemQuery: Parameters: {@QueryParams}", Parameters);
            foreach (var item in Parameters)
            {
                Log.Debug("WorkItemQuery: {0}: {1}", item.Key, item.Value);
            }
            return GetWorkItemsFromQuery(wiClient);
        }

        private IList<WorkItem> GetWorkItemsFromQuery(TfsWorkItemMigrationClient wiClient)
        {
            var startTime = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            var results = new List<WorkItem>();
            try
            {
                Log.Debug("Query sent");
                var workItemCollection = wiClient.Store.Query(Query);
                if (workItemCollection.Count > 0)
                {
                    Log.Information("{0} Work items received, verifying", workItemCollection.Count);
                    foreach (WorkItem item in workItemCollection)
                    {
                        int id= 0;
                        try
                        {
                            id = item.Id;
                            if (!string.IsNullOrEmpty(item.Title)) // Force to read WI
                                results.Add(item);
                        }
                        catch (DeniedOrNotExistException ex)
                        {
                           
                            Log.Warning(ex, "The Work Item {id} cant be accessed for some reason and returned a DeniedOrNotExistException! The specific error will be listed below.", id);
                            Telemetry.TrackException(ex, 
                                                               new Dictionary<string, string>
                                                               {
                                    { "CollectionUrl", wiClient.Store.TeamProjectCollection.Uri.ToString() }
                                },
                                                                                              new Dictionary<string, double>
                                                                                              {
                                    { "QueryTime",timer.ElapsedMilliseconds }
                                });
                        }
                    }
                }
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Options.Collection.ToString(), "GetWorkItemsFromQuery", null, startTime, timer.Elapsed, "200", true));
            }
            catch (ValidationException ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Options.Collection.ToString(), "GetWorkItemsFromQuery", null, startTime, timer.Elapsed, "500", false));
                Log.Error(ex, " Error running query");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Options.Collection.ToString(), "GetWorkItemsFromQuery", null, startTime, timer.Elapsed, "500", false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", wiClient.Store.TeamProjectCollection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "QueryTime",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, " Error running query");
                throw;
            }
            return results;
        }
    }
}