using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public interface IProcessorEnricher : IEnricher
    {
        void ProcessorExecutionBegin(IProcessor2 processor);

        void ProcessorExecutionEnd(IProcessor2 processor);

        void ProcessorExecutionAfterSource(IProcessor2 processor, List<WorkItemData2> workItems);

        void ProcessorExecutionAfterProcessWorkItem(IProcessor2 processor, WorkItemData2 workitem);

        void ProcessorExecutionBeforeProcessWorkItem(IProcessor2 processor, WorkItemData2 workitem);

        void Configure(IProcessorEnricherOptions options);
    }
}