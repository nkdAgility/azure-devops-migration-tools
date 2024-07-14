using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
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

        public AppendMigrationToolSignatureFooter(IServiceProvider services, ILogger<WorkItemProcessorEnricher> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
        {
            Engine = Services.GetRequiredService<IMigrationEngine>();
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new System.NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (AppendMigrationToolSignatureFooterOptions)options;
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