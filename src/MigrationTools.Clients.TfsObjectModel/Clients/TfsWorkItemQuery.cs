using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Services;
using Serilog;

namespace MigrationTools.Clients
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("TfsWorkItemQuery:GetInternalWorkItems", ActivityKind.Internal))
            {
                activity?.SetTagsFromOptions(MigrationClient.Options);
                activity?.SetTag("url.full", MigrationClient.Options.Collection);
                activity?.SetTag("server.address", MigrationClient.Options.Collection);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
                foreach (var item in Parameters)
                {
                    activity?.SetTag($"wiql.parameters.{item.Key}", item.Value);
                }

                Log.Debug("WorkItemQuery: ===========GetWorkItems=============");
                var wiClient = (TfsWorkItemMigrationClient)MigrationClient.WorkItems;
                Log.Debug("WorkItemQuery: TeamProjectCollection: {QueryTarget}", wiClient.Store.TeamProjectCollection.Uri.ToString());
                Log.Debug("WorkItemQuery: Query: {QueryText}", Query);
                Log.Debug("WorkItemQuery: Parameters: {@QueryParams}", Parameters);
                foreach (var item in Parameters)
                {
                    Log.Debug("WorkItemQuery: {0}: {1}", item.Key, item.Value);
                }
                return GetWorkItemsFromQuery(wiClient);
            }
        }

        private IList<WorkItem> GetWorkItemsFromQuery(TfsWorkItemMigrationClient wiClient)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItemsFromQuery", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(MigrationClient.Options);
                activity?.SetTag("url.full", MigrationClient.Options.Collection);
                activity?.SetTag("server.address", MigrationClient.Options.Collection);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
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
                            int id = 0;
                            try
                            {
                                id = item.Id;
                                if (!string.IsNullOrEmpty(item.Title)) // Force to read WI
                                    results.Add(item);
                            }
                            catch (DeniedOrNotExistException ex)
                            {

                                Log.Warning(ex, "The Work Item {id} cant be accessed for some reason and returned a DeniedOrNotExistException! The specific error will be listed below.", id);
                                Telemetry.TrackException(ex, activity.Tags);
                            }
                        }
                    }
                    activity?.SetTag("http.response.status_code", "200");
                }
                catch (ValidationException ex)
                {
                    activity?.SetTag("http.response.status_code", "500");
                    Log.Error(ex, " Error running query");
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {

                    activity?.SetTag("http.response.status_code", "500");
                    Telemetry.TrackException(ex, activity.Tags);
                    Log.Error(ex, " Error running query");
                    throw;
                }
                return results;
            }
        }
    }
}