using MigrationTools.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Sinks
{
    public interface IWorkItemSink
    {
        IEnumerable<WorkItemData> GetWorkItems();
        WorkItemData PersistWorkItem(WorkItemData workItem);
    }
}
