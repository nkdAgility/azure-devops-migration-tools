using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tools
{
    public class CommonTools
    {
        public StringManipulatorTool StringManipulator { get; private set; }
        public WorkItemTypeMappingTool WorkItemTypeMapping { get; private set; }

        public FieldMappingTool FieldMappingTool { get; private set; }
        public CommonTools(StringManipulatorTool StringManipulatorTool, WorkItemTypeMappingTool workItemTypeMapping, FieldMappingTool fieldMappingTool)
        {
            StringManipulator = StringManipulatorTool;
            WorkItemTypeMapping = workItemTypeMapping;
            FieldMappingTool = fieldMappingTool;
        }

    }
}
