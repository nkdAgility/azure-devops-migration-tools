using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace TfsWitMigrator.Core
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
            WorkItemCollection wc;
            var ai = new TelemetryClient();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                wc = storeContext.Store.Query(Query, parameters);
                ai.TrackMetric("QueryCount", wc.Count);
                sw.Stop();
                ai.TrackRequest("TFS Query", DateTime.Now, sw.Elapsed, "200", true);
            }
            catch (Exception ex)
            {
                ai.TrackRequest("TFS Query", DateTime.Now, sw.Elapsed, "500", false);
                ai.TrackException(ex);
                throw;
            }
            return wc;
        }
    }
}