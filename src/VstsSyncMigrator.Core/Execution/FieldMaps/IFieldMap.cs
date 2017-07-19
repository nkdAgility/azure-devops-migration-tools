using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.ComponentContext
{
   public interface IFieldMap
    {
        string Name { get; }

        void Init(MigrationEngine me, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore);

        void Execute(WorkItem source, WorkItem target);
    }
}
