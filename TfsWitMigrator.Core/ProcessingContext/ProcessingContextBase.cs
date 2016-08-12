using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace VSTS.DataBulkEditor.Engine
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
            Telemetry.Current.TrackEvent("ProcessingContextExecute",
                     new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", me.Target.Name},
                          { "Target Collection", me.Target.Collection.Name }
                     });
            Trace.TraceInformation(string.Format("ProcessingContext Start {0} ", Name));
            Stopwatch executeTimer = new Stopwatch();
            executeTimer.Start();
            //////////////////////////////////////////////////
            try
            {
                status = ProcessingStatus.Running;
                InternalExecute();
                status = ProcessingStatus.Complete;
                executeTimer.Stop();
                Telemetry.Current.TrackEvent("ProcessingContextComplete",
                    new Dictionary<string, string> {
                        { "Name", Name},
                        { "Target Project", me.Target.Name},
                        { "Target Collection", me.Target.Collection.Name },
                        { "Status", Status.ToString() }
                    },
                    new Dictionary<string, double> {
                        { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                    });
                Trace.TraceInformation(string.Format("ProcessingContext Complete {0} ", Name));
            }
            catch (Exception ex)
            {
                status = ProcessingStatus.Failed;
                executeTimer.Stop();
                Telemetry.Current.TrackException(ex,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", me.Target.Name},
                          { "Target Collection", me.Target.Collection.Name },
                          { "Status", Status.ToString() }
                      },
                      new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                      });
                Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
            }
        }

        internal abstract void InternalExecute();

    }
}