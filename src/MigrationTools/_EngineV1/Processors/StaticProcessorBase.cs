using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools.Configuration;
using MigrationTools.Processors;

namespace VstsSyncMigrator._EngineV1.Processors
{
    public abstract class StaticProcessorBase : MigrationTools._EngineV1.Containers.IProcessor
    {
        internal IMigrationEngine _me;
        private ProcessingStatus status = ProcessingStatus.None;
        private readonly IServiceProvider _services;

        public IMigrationEngine Engine { get { return _me; } }
        public IServiceProvider Services { get { return _services; } }

        public StaticProcessorBase(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<MigrationTools._EngineV1.Containers.IProcessor> logger)
        {
            _services = services;
            _me = me;
            Telemetry = telemetry;
            Log = logger;
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
        public ILogger<MigrationTools._EngineV1.Containers.IProcessor> Log { get; }

        public void Execute()
        {
            Telemetry.TrackEvent(Name);
            Log.LogDebug("StaticProcessorBase: Start {0} ", Name);
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
                        { "Target", Engine.Target.Config.ToString()},
                        { "Status", Status.ToString() }
                    },
                    new Dictionary<string, double> {
                        { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                    });
                Log.LogDebug("StaticProcessorBase: ProcessingContext Complete {0} ", Name);
            }
            catch (Exception ex)
            {
                status = ProcessingStatus.Failed;
                executeTimer.Stop();
                Telemetry.TrackException(ex,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target", Engine.Target.Config.ToString()},
                          { "Status", Status.ToString() }
                      },
                      new Dictionary<string, double> {
                            { "ProcessingContextTime", executeTimer.ElapsedMilliseconds }
                      });
                Log.LogCritical(ex, "Processing Context failed.");
            }
            finally
            {
                Telemetry.TrackRequest(Name, start, executeTimer.Elapsed, Status.ToString(), Status == ProcessingStatus.Complete);
            }
        }

        protected abstract void InternalExecute();
    }
}