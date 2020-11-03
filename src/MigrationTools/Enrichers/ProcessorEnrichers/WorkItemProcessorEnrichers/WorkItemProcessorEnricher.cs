using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

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

        public void ProcessorExecutionBegin(IProcessor2 processor)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionBegin::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionEnd(IProcessor2 processor)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionEnd::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionAfterSource(IProcessor2 processor, List<WorkItemData2> workItems)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionAfterSource::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionAfterProcessWorkItem(IProcessor2 processor, WorkItemData2 workitem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionAfterProcessWorkItem::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionBeforeProcessWorkItem(IProcessor2 processor, WorkItemData2 workitem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionBeforeProcessWorkItem::NoAction", this.GetType().Name);
        }
    }
}