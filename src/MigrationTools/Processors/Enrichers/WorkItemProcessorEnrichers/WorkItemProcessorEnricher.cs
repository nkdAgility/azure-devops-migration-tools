using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Enrichers
{
    [Obsolete]
    public abstract class WorkItemProcessorEnricher : IWorkItemProcessorEnricher
    {
        protected ITelemetryLogger Telemetry { get; }
        protected IServiceProvider Services { get; }
        protected ILogger<IWorkItemProcessorEnricher> Log { get; }

        public WorkItemProcessorEnricher(IServiceProvider services, ILogger<WorkItemProcessorEnricher> logger, ITelemetryLogger telemetry)
        {
            Services = services;
            Log = logger;
            Telemetry = telemetry;
        }

        protected abstract void RefreshForProcessorType(IProcessor processor);

        protected abstract void EntryForProcessorType(IProcessor processor);

        [Obsolete("v1 Architecture: Here to support migration, use PhaseEnrichers: BeforeLoadData, AfterLoadData, etc", false)]
        public virtual int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new InvalidOperationException("This is invalid for this Enricher type");
        }

        public virtual void ProcessorExecutionBegin(IProcessor processor)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionBegin::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionEnd(IProcessor processor)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionEnd::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionAfterSource(IProcessor processor, List<WorkItemData> workItems)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionAfterSource::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionAfterProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionAfterProcessWorkItem::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionBeforeProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionBeforeProcessWorkItem::NoAction", this.GetType().Name);
        }

        public virtual void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::NoAction", this.GetType().Name);
        }
    }
}