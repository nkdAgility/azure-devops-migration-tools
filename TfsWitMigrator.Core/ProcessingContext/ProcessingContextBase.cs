using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace VSTS.DataBulkEditor.Core
{
    public abstract class ProcessingContextBase : ITfsProcessingContext
    {
        internal MigrationEngine me;
        ProcessingStatus status = ProcessingStatus.None;
        public MigrationEngine Engine { get { return me; } }

        public ProcessingContextBase(MigrationEngine me)
        {
            this.me = me;
        }

        public abstract string Name { get; }

        public ProcessingStatus Status
        {
            get
            {
                return status;
            }
        }

        public void Execute()
        {
            TelemetryClient tc = new TelemetryClient();
            tc.TrackEvent(string.Format("Execute: {0}", this.Name));
            var properties = new Dictionary<string, string> {
                    { "Processing Engine", this.Name},
                    { "Target Project", me.Target.Name},
                    { "Target Collection", me.Target.Collection.Name }
                };
            var measurements = new Dictionary<string, double>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Trace.WriteLine("");
            //////////////////////////////////////////////////
            try
            {
                status = ProcessingStatus.Running;
                InternalExecute();
                status = ProcessingStatus.Complete;
                stopwatch.Stop();
                Trace.WriteLine(string.Format(@"EXECUTE DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed));
                measurements.Add("ExecutionTime-Milliseconds", stopwatch.ElapsedMilliseconds);
                tc.TrackEvent(string.Format("Complete: {0}", this.GetType().Name), properties, measurements);
            }
            catch (Exception ex)
            {
                status = ProcessingStatus.Failed;
                stopwatch.Stop();
                measurements.Add("ExecutionTime-Milliseconds", stopwatch.ElapsedMilliseconds);
                // Send the exception telemetry:
                tc.TrackException(ex, properties, measurements);
                Trace.TraceError(ex.ToString());
            }
        }

        internal abstract void InternalExecute();

    }
}