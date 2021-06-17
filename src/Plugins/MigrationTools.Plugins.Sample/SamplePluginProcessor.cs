using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Helpers;
using MigrationTools.Processors;

namespace MigrationTools.Plugins.Sample
{
    public class SamplePluginProcessorOptions : IProcessorConfig, IProcessorOptions
    {
        public string Greeting { get; set; } = "Howdy";
        public bool Enabled { get; set; }

        public string Processor => nameof(SamplePluginProcessor);

        public string SourceName { get; set; }

        public string TargetName { get; set; }

        public List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }
        public string RefName { get; set; }

        public Type ToConfigure => typeof(SamplePluginProcessor);
        public IProcessorOptions GetDefault() => new SamplePluginProcessorOptions();
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors) => true;
        public void SetDefaults() { }
    }
    public class SamplePluginProcessor : Processor, IProcessor, IPluginProcessor
    {
        private SamplePluginProcessorOptions _Options;

        [PluginConstructorAttribute]
        public SamplePluginProcessor(
            ProcessorEnricherContainer processorEnrichers,
            IEndpointFactory endpointFactory,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<SamplePluginProcessor> logger) : base(processorEnrichers, endpointFactory, services, telemetry, logger) { }

        protected override void InternalExecute()
        {
            Log.LogInformation($"{_Options.Greeting} from the sample plugin");
        }

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("SamplePluginProcessor::Configure");
            _Options = (SamplePluginProcessorOptions)options;
        }

    }
}
