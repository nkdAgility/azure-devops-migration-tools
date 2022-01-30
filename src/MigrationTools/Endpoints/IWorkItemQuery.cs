using System.Collections.Generic;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemQuery
    {
        string Query { get; }

        void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters);

        List<WorkItemData> GetWorkItems();
    }
}