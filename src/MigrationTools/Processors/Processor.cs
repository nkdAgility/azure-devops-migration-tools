using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public abstract class Processor : IProcessor2
    {
        public Processor(IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public Collection<IEndpoint> Endpoints => throw new NotImplementedException();

        public Collection<IProcessorEnricher> Enrichers => throw new NotImplementedException();

        public string Name { get { return this.GetType().Name; } }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;
        public IServiceProvider Services { get; }
        public ITelemetryLogger Telemetry { get; }
        public ILogger<Processor> Log { get; }

        public abstract void Configure(IProcessorOptions config);

        public void Configure(IProcessorConfig config)
        {
            Configure((IProcessorOptions)config);
        }

        public void Execute()
        {
            Telemetry.TrackEvent(this.Name);
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

                Log.LogInformation(" Migration Processor Complete {MigrationContextname} ", Name);
            }
            catch (Exception ex)
            {
                Status = ProcessingStatus.Failed;
                executeTimer.Stop();

                Telemetry.TrackException(ex,
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        //{"Target", Engine.Target.Config.ToString()},
                        //{"Source", Engine.Source.Config.ToString()},
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
                Log.LogInformation("{ProcessorName} completed in {ProcessorDuration} ", Name, executeTimer.Elapsed.ToString("c"));
                Telemetry.TrackRequest(this.Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }
        }

        protected abstract void InternalExecute();
    }
}