using System.Collections.Generic;
using System.Text;
using MigrationTools.Core.DataContracts;

namespace MigrationTools.Core.Clients
{
    public interface IWorkItemQuery
    {
        void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters);
         List<WorkItemData> GetWorkItems();
    }
}