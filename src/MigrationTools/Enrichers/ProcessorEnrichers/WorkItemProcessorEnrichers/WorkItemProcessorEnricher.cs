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

        public abstract void Configure(bool save = true, bool filter = true);

        public abstract int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}