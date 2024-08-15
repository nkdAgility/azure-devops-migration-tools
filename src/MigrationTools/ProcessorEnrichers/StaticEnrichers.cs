using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;

namespace MigrationTools.ProcessorEnrichers
{
    public class StaticEnrichers
    {
        public StringManipulatorEnricher StringManipulator { get; private set; }
        public WorkItemTypeMappingEnricher WorkItemTypeMapping { get; private set; }
        public StaticEnrichers(StringManipulatorEnricher stringManipulatorEnricher, WorkItemTypeMappingEnricher workItemTypeMapping)
        {
            StringManipulator = stringManipulatorEnricher;
            WorkItemTypeMapping = workItemTypeMapping;
        }

    }
}
