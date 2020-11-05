using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public interface IProcessorEnricher : IEnricher
    {
        void ProcessorExecutionBegin(IProcessor2 processor);

        void ProcessorExecutionEnd(IProcessor2 processor);

        void ProcessorExecutionAfterSource(IProcessor2 processor, List<WorkItemData> workItems);

        void ProcessorExecutionAfterProcessWorkItem(IProcessor2 processor, WorkItemData workitem);

        void ProcessorExecutionBeforeProcessWorkItem(IProcessor2 processor, WorkItemData workitem);

        void Configure(IProcessorEnricherOptions options);
    }
}