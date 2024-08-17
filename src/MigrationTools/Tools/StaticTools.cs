using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tools
{
    public class StaticTools
    {
        public StringManipulatorTool StringManipulator { get; private set; }
        public WorkItemTypeMappingTool WorkItemTypeMapping { get; private set; }

        public FieldMappingTool FieldMappingTool { get; private set; }
        public StaticTools(StringManipulatorTool StringManipulatorTool, WorkItemTypeMappingTool workItemTypeMapping, FieldMappingTool fieldMappingTool)
        {
            StringManipulator = StringManipulatorTool;
            WorkItemTypeMapping = workItemTypeMapping;
            FieldMappingTool = fieldMappingTool;
        }

    }
}
