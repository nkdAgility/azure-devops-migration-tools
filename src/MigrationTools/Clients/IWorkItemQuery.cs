using System.Collections.Generic;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public interface IWorkItemQuery
    {
        void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters);

        List<WorkItemData> GetWorkItems();
    }
}