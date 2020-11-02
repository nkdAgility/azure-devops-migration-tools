using System;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public abstract class WorkItemProcessorEnricher : IWorkItemProcessorEnricher
    {
        protected IMigrationEngine Engine { get; }
        protected ILogger<IWorkItemProcessorEnricher> Log { get; }

        public WorkItemProcessorEnricher(IMigrationEngine engine, ILogger<WorkItemProcessorEnricher> logger)
        {
            Engine = engine;
            Log = logger;
        }

        [Obsolete("v1 Architecture: Here to support migration, use Configure(IProcessorEnricherOptions options) instead", false)]
        public abstract void Configure(bool save = true, bool filter = true);

        public abstract void Configure(IProcessorEnricherOptions options);

        [Obsolete("v1 Architecture: Here to support migration, use PhaseEnrichers: BeforeLoadData, AfterLoadData, etc", false)]
        public abstract int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}