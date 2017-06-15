using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine
{
    public class TfsQueryContext
    {
        private WorkItemStoreContext storeContext;
        private Dictionary<string, string> parameters;

        public TfsQueryContext(WorkItemStoreContext storeContext)
        {
            this.storeContext = storeContext;
            parameters = new Dictionary<string, string>();
        }

        public string Query { get; set; }

        public void AddParameter(string name, string value)
        {
            parameters.Add(name, value);
        }

        public WorkItemCollection Execute()
        {
            Trace.WriteLine(string.Format("TfsQueryContext: {0}: {1}", "TeamProjectCollection", storeContext.Store.TeamProjectCollection.Uri.ToString()), "TfsQueryContext");
            WorkItemCollection wc;
            var startTime = DateTime.UtcNow;
            Stopwatch queryTimer = new Stopwatch();
            foreach (var item in parameters)
            {
                Trace.WriteLine(string.Format("TfsQueryContext: {0}: {1}", item.Key, item.Value), "TfsQueryContext");
            }           

            queryTimer.Start();
            try
            {
                wc = storeContext.Store.Query(Query, parameters);
                queryTimer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "Query", startTime, queryTimer.Elapsed, true);
                // Add additional bits to reuse the paramiters dictionary for telemitery
                parameters.Add("CollectionUrl", storeContext.Store.TeamProjectCollection.Uri.ToString());
                parameters.Add("Query", Query);
                Telemetry.Current.TrackEvent("QueryComplete",
                      parameters,
                      new Dictionary<string, double> {
                            { "QueryTime", queryTimer.ElapsedMilliseconds },
                          { "QueryCount", wc.Count }
                      });
                Trace.TraceInformation(string.Format(" Query Complete: found {0} work items in {1}ms ", wc.Count, queryTimer.ElapsedMilliseconds));
         
        }
            catch (Exception ex)
            {
                queryTimer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "Query", startTime, queryTimer.Elapsed, false);
                Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", storeContext.Store.TeamProjectCollection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "QueryTime",queryTimer.ElapsedMilliseconds }
                       });
                //Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
                //Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.StackTrace));
                throw ex;
            }
            return wc;
        }
    }
}