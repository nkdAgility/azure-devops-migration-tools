using MigrationTools.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Clients
{
   public  interface IWorkItemMigrationClients
    {
        IEnumerable<WorkItemData> GetWorkItems();
        WorkItemData PersistWorkItem(WorkItemData workItem);
    }
}
