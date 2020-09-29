using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MigrationTools.Core.Clients
{
   public  interface IMigrationClient
    {

        TeamProjectConfig Config { get; }
        void Configure(TeamProjectConfig config, NetworkCredential credentials = null);

          T GetService<T>();

        [Obsolete]
        object InternalCollection { get; }

        IEnumerable<WorkItemData> GetWorkItems();
        WorkItemData PersistWorkItem(WorkItemData workItem);

    }
}
