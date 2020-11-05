using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools.Configuration;
using MigrationTools.Processors;

namespace MigrationTools._EngineV1.Processors
{
    public abstract class MigrationProcessorBase : Containers.IProcessor
    {
        protected MigrationProcessorBase(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<MigrationProcessorBase> logger)
        {
            Engine = engine;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        protected IMigrationEngine Engine { get; }

        protected ILogger<MigrationProcessorBase> Log { get; }

        protected IServiceProvider Services { get; }

        protected ITelemetryLogger Telemetry { get; }

        public abstract void Configure(IProcessorConfig config);

        public void Execute()
        {
            Telemetry.TrackEvent(Name);
            Log.LogInformation("Migration Context Start: {MigrationContextname} ", Name);
            DateTime start = DateTime.Now;
            var executeTimer = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            try
            {
                Status = ProcessingStatus.Running;
                InternalExecute();
                Status = ProcessingStatus.Complete;
                executeTimer.Stop();

                Log.LogInformation(" Migration Context Complete {MigrationContextname} ", Name);
            }
            catch (Exception ex)
            {
                Status = ProcessingStatus.Failed;
                executeTimer.Stop();

                Telemetry.TrackException(ex,
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        {"Target", Engine.Target.Config.ToString()},
                        {"Source", Engine.Source.Config.ToString()},
                        {"Status", Status.ToString()}
                    },
                    new Dictionary<string, double>
                    {
                        {"MigrationContextTime", executeTimer.ElapsedMilliseconds}
                    });
                Log.LogCritical(ex, "Error while running {MigrationContextname}", Name);
            }
            finally
            {
                Telemetry.TrackRequest(Name, start, executeTimer.Elapsed, Status.ToString(), Status == ProcessingStatus.Complete);
            }
        }

        protected static void AddMetric(string name, IDictionary<string, double> store, double value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        protected static void AddParameter(string name, IDictionary<string, string> store, string value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        protected abstract void InternalExecute();
    }
}