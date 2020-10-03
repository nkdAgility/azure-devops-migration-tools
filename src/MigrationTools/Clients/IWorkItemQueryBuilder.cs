using System.Collections.Generic;

namespace MigrationTools.Clients
{
    public interface IWorkItemQueryBuilder
    {
        Dictionary<string, string> Parameters { get; }
        string Query { get; set; }
        void AddParameter(string name, string value);

        IWorkItemQuery BuildWIQLQuery(IMigrationClient migrationClient);

    }
}
