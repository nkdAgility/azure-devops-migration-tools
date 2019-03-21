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

        // Fix for Query SOAP error when passing parameters
        [Obsolete("Temporary work aorund for SOAP issue https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066")]
        string WorkAroundForSOAPError(string query, IDictionary<string, string> parameters)
        {
            foreach (string key in parameters.Keys)
            {
                string pattern = "'{0}'";
                if (IsInteger(parameters[key]))
                {
                    pattern = "{0}";
                }
                query = query.Replace(string.Format("@{0}", key), string.Format(pattern, parameters[key]));
            }
            return query;
        }

        public bool IsInteger(string maybeInt)
        {
            int testNumber = 0;
            //Check whether 'first' is integer
            return int.TryParse(maybeInt, out testNumber);
        }

        public WorkItemCollection Execute()
        {
            Telemetry.Current.TrackEvent("TfsQueryContext.Execute",parameters);
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
                //Query = WorkAroundForSOAPError(Query, parameters); // TODO: Remove this once bug fixed... https://dev.azure.com/nkdagility/migration-tools/_workitems/edit/5066 
                wc = storeContext.Store.Query(Query, parameters);
                queryTimer.Stop();
                Telemetry.Current.TrackDependency("AzureDevOps", "AzureBoards", "Query", startTime, queryTimer.Elapsed, true);
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
                Telemetry.Current.TrackDependency("AzureDevOps", "AzureBoards", "Query", startTime, queryTimer.Elapsed, false);
                Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", storeContext.Store.TeamProjectCollection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "QueryTime",queryTimer.ElapsedMilliseconds }
                       });
                Trace.TraceWarning($"  [EXCEPTION] {ex}");
                throw;
            }
            return wc;
        }
    }
}