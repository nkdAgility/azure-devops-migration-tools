using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace VSTS.DataBulkEditor.Engine
{
    public abstract class MigrationContextBase : ITfsProcessingContext
    {
        internal MigrationEngine me;
        ProcessingStatus status = ProcessingStatus.None;


        public MigrationContextBase(MigrationEngine me)
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
            tc.TrackPageView(this.Name);

            tc.TrackEvent(string.Format("{0}", this.Name));
            var properties = new Dictionary<string, string> {
                    { "Processing Engine", this.Name},
                    { "Target Project", me.Target.Name},
                    { "Target Collection", me.Target.Collection.Name },
                     { "Source Project", me.Source.Name},
                    { "Source Collection", me.Source.Collection.Name }
                };
            var measurements = new Dictionary<string, double>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            try
            {
                status = ProcessingStatus.Running;
                InternalExecute();
                status = ProcessingStatus.Complete;
                stopwatch.Stop();
                tc.TrackMetric("ExecutionTime", stopwatch.ElapsedMilliseconds);
                measurements.Add("ExecutionTime-Milliseconds", stopwatch.ElapsedMilliseconds);
                tc.TrackEvent(string.Format("{0}:{1}", this.GetType().Name, status.ToString()), properties, measurements);
              
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

        internal string NodeStructreSourceToTarget(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            return string.Format("{0}\\{1}", me.Target.Name, input);


            //Regex r = new Regex(source.Name, RegexOptions.IgnoreCase);


            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            //return r.Replace(input, target.Name, 1);

        }

        internal string ReplaceFirstInstanceOf(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            Regex r = new Regex(me.Source.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, me.Target.Name, 1);

        }
    }



}