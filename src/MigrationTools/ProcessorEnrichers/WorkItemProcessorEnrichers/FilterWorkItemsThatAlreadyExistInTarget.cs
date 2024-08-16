using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Options;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class FilterWorkItemsThatAlreadyExistInTarget : WorkItemProcessorEnricher
    {
        private FilterWorkItemsThatAlreadyExistInTargetOptions _Options;

        public FilterWorkItemsThatAlreadyExistInTargetOptions Options
        {
            get { return _Options; }
        }

        public IMigrationEngine Engine { get; private set; }

        public FilterWorkItemsThatAlreadyExistInTarget(IOptions<FilterWorkItemsThatAlreadyExistInTargetOptions> options, IServiceProvider services, ILogger<FilterWorkItemsThatAlreadyExistInTarget> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
        {
            _Options = options.Value;
            Engine = Services.GetRequiredService<IMigrationEngine>();
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new System.NotImplementedException();
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