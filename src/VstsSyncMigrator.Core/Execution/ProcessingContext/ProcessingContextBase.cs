using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using MigrationTools.Core.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public abstract class ProcessingContextBase : ITfsProcessingContext
    {
        internal MigrationEngine me;
        ProcessingStatus status = ProcessingStatus.None;
        public MigrationEngine Engine { get { return me; } }

        public ProcessingContextBase(MigrationEngine me, ITfsProcessingConfig config)
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
            Telemetry.Current.TrackPageView(this.Name);
            Trace.TraceInformation(string.Format("ProcessingContext Start {0} ", Name));
            Stopwatch executeTimer = Stopwatch.StartNew();
			DateTime start = DateTime.Now;
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
                        { "Target Project", me.Target.Config.Project},
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
                          { "Target Project", me.Target.Config.Project},
                          { "Target Collection", me.Target.Collection.Name },
                          { "Status", Status.ToString() }
                      },
                      new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                      });
                Trace.TraceWarning($"  [EXCEPTION] {ex}");
            }
            finally
            {
                Telemetry.Current.TrackRequest(this.Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }
        }

        internal abstract void InternalExecute();

    }
}