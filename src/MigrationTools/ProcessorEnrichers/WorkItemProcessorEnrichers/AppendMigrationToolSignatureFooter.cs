using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Options;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class AppendMigrationToolSignatureFooter : WorkItemProcessorEnricher
    {
        private AppendMigrationToolSignatureFooterOptions _Options;

        public AppendMigrationToolSignatureFooterOptions Options
        {
            get { return _Options; }
        }

        public IMigrationEngine Engine { get; }

        public AppendMigrationToolSignatureFooter(IOptions<AppendMigrationToolSignatureFooterOptions> options, IServiceProvider services, ILogger<WorkItemProcessorEnricher> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
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