using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class Processor : IProcessor
    {
        private bool _ProcessorConfigured;
        private IEndpoint _source;
        private IEndpoint _target;

        public Processor(
            IOptions<ProcessorOptions> options,
            StaticTools staticTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<Processor> logger)
        {
            Options = options.Value;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
            ProcessorEnrichers = processorEnrichers;
            StaticTools = staticTools;
        }

        public StaticTools StaticTools { get; private set; }

        public IProcessorOptions Options { get; private set; }

        public IEndpoint Source { get { if (_source == null) { _source = Services.GetKeyedService<IEndpoint>(Options.SourceName); } return _source; } }
        public IEndpoint Target { get { if (_target == null) { _target = Services.GetKeyedService<IEndpoint>(Options.TargetName); } return _target; } }

        public ProcessorEnricherContainer ProcessorEnrichers { get; }

        public string Name { get { return GetType().Name; } }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<Processor> Log { get; }

        public bool SupportsProcessorEnrichers => false;

        public virtual ProcessorType Type => ProcessorType.AddHock;

        public void Execute()
        {
            Telemetry.TrackEvent(this.Name);
            Log.LogInformation("Migration Context Start: {MigrationContextname} ", Name);
            DateTime start = DateTime.Now;
            var executeTimer = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            try
            {
                if (!_ProcessorConfigured)
                {
                    Log.LogError("Processor::Execute: Processer base has not been configured.");
                    throw new InvalidOperationException("Processer base has not been configured.");
                }
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
                Telemetry.TrackRequest(Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }
        }

        protected abstract void InternalExecute();

        protected Type GetTypeFromName(string name)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                 .Where(a => !a.IsDynamic)
                 .SelectMany(a => a.GetTypes())
                 .FirstOrDefault(t => t.Name.Equals(name) || t.FullName.Equals(name));
            if (type is null)
            {
                var e = new InvalidOperationException("The Type cant be found");
                Log.LogError(e, "Unable to find the type {ObjectToLoadName} needed by the processor {ProfessorName}", name, nameof(this.GetType));
                throw e;
            }
            return type;
        }

        protected static void AddMetric(string name, IDictionary<string, double> store, double value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        protected static void AddParameter(string name, IDictionary<string, string> store, string value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }
    }
}