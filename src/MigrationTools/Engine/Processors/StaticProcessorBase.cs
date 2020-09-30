using System;
using System.Collections.Generic;
using System.Diagnostics;
using MigrationTools.Configuration;
using MigrationTools;
using Serilog;
using MigrationTools.Engine.Containers;
using MigrationTools;

namespace VstsSyncMigrator.Engine
{
    public abstract class StaticProcessorBase : IProcessor
    {
        internal IMigrationEngine _me;
        ProcessingStatus status = ProcessingStatus.None;
        private readonly IServiceProvider _services;

        public IMigrationEngine Engine { get { return _me; } }
        public IServiceProvider Services { get { return _services; } }

        public StaticProcessorBase(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry)
        {
            _services = services;
            _me = me;
            Telemetry = telemetry;
        }

        public abstract void Configure(IProcessorConfig config);

        public abstract string Name { get; }

        public ProcessingStatus Status
        {
            get
            {
                return status;
            }
        }

        public ITelemetryLogger Telemetry { get; }

        public void Execute()
        {
            Telemetry.TrackEvent(this.Name);
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
                Telemetry.TrackEvent("ProcessingContextComplete",
                    new Dictionary<string, string> {
                        { "Name", Name},
                        { "Target Project", Engine.Target.Config.Project},
                        { "Target Collection", Engine.Target.Config.Collection.ToString()},
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
                Telemetry.TrackException(ex,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", Engine.Target.Config.Project},
                          { "Target Collection", Engine.Target.Config.Collection.ToString() },
                          { "Status", Status.ToString() }
                      },
                      new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                      });
                Log.Fatal(ex, "Processing Context failed.");
            }
            finally
            {
                Telemetry.TrackRequest( this.Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }
        }

        protected abstract void InternalExecute();

        
    }
}