using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    public class CommonTools
    {
        public IStringManipulatorTool StringManipulator { get; private set; }
        public IWorkItemTypeMappingTool WorkItemTypeMapping { get; private set; }

        public IFieldMappingTool FieldMappingTool { get; private set; }
        public CommonTools(IStringManipulatorTool StringManipulatorTool, IWorkItemTypeMappingTool workItemTypeMapping, IFieldMappingTool fieldMappingTool)
        {
            StringManipulator = StringManipulatorTool;
            WorkItemTypeMapping = workItemTypeMapping;
            FieldMappingTool = fieldMappingTool;
        }

    }
}
