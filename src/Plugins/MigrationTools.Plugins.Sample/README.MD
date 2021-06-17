# Processor Plugins System
There are times where you might want to write custom processors that you can slip into an "out of the box" installation of the tool.

This allows developers to write, use and maintain processors outside of the existing NkdAgility repository.

## Reference Sample Plugin

```cs
    public class SamplePluginProcessorOptions : ProcessorOptions, IProcessorConfig
    {
        public string Greeting { get; set; } = "Howdy";

        public override Type ToConfigure => typeof(SamplePluginProcessor);
        public override IProcessorOptions GetDefault() => this;
        public override void SetDefaults() { }
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
```

One key thing to note is that from inside your sample project you will need to reference the MigrationTools.csproj from NkdAgility. You can use git submodules to pull in the repos, or just grab the an existing MigrationTools.dll.