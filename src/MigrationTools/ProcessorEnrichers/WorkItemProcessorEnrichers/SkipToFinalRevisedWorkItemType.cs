using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class SkipToFinalRevisedWorkItemType : WorkItemProcessorEnricher
    {
        private StringManipulatorEnricherOptions _Options;

        public StringManipulatorEnricherOptions Options
        {
            get { return _Options; }
        }

        public IMigrationEngine Engine { get; private set; }

        public SkipToFinalRevisedWorkItemType(IServiceProvider services, ILogger<SkipToFinalRevisedWorkItemType> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
        {
            Engine = Services.GetRequiredService<IMigrationEngine>();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (StringManipulatorEnricherOptions)options;
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

    }
}