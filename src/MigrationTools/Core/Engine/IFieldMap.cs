using MigrationTools.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Engine
{
    interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        void Execute(WorkItemData source, WorkItemData target);
    }
}
