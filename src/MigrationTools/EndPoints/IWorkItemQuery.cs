using System;
using System.Collections.Generic;
using MigrationTools.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemQuery
    {
        string Query { get; }

        void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters);

        [Obsolete("For old style code use this, for new style use GetWorkItems2. Return type differs only")]
        List<WorkItemData> GetWorkItems();

        List<WorkItemData2> GetWorkItems2();
    }
}