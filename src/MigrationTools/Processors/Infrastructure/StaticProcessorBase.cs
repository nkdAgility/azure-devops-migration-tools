using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors;
using MigrationTools.Tools;

namespace VstsSyncMigrator._EngineV1.Processors
{
    public abstract class StaticProcessorBase : MigrationTools._EngineV1.Containers.IProcessor
    {
        protected IMigrationEngine Engine { get; }
        protected IServiceProvider Services { get; }

        public StaticTools StaticEnrichers { get; private set; }

        public StaticProcessorBase(StaticTools staticEnrichers,IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<StaticProcessorBase> logger)
        {
            Services = services;
            Engine = me;
            Telemetry = telemetry;
            Log = logger;
            StaticEnrichers = staticEnrichers;
        }

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        public ITelemetryLogger Telemetry { get; }
        public ILogger<StaticProcessorBase> Log { get; }

        public ProcessorType Type => ProcessorType.Legacy;

        public void Execute()
        {
            Telemetry.TrackEvent(Name);
            Log.LogDebug("StaticProcessorBase: Start {0} ", Name);
            Stopwatch executeTimer = Stopwatch.StartNew();
            DateTime start = DateTime.Now;
            //////////////////////////////////////////////////
            try
            {
                Status = ProcessingStatus.Running;
                InternalExecute();
                Status = ProcessingStatus.Complete;
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
                Status = ProcessingStatus.Failed;
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