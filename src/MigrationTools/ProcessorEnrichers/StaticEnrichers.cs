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

        public FieldMappingTool FieldMappingTool { get; private set; }
        public StaticEnrichers(StringManipulatorEnricher stringManipulatorEnricher, WorkItemTypeMappingEnricher workItemTypeMapping, FieldMappingTool fieldMappingTool)
        {
            StringManipulator = stringManipulatorEnricher;
            WorkItemTypeMapping = workItemTypeMapping;
            FieldMappingTool = fieldMappingTool;
        }

    }
}
