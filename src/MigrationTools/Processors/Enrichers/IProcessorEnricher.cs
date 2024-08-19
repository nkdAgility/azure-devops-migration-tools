using System;
using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Enrichers
{
    public interface IProcessorEnricher : IEnricher
    {
        void ProcessorExecutionBegin(IProcessor processor);

        void ProcessorExecutionEnd(IProcessor processor);

        void ProcessorExecutionAfterSource(IProcessor processor, List<WorkItemData> workItems);

        void ProcessorExecutionAfterProcessWorkItem(IProcessor processor, WorkItemData workitem);

        void ProcessorExecutionBeforeProcessWorkItem(IProcessor processor, WorkItemData workitem);

        void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem);
    }
}