using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools._EngineV1.Clients
{
    public interface IWorkItemQueryBuilder
    {
        string Query { get; set; }

        void AddParameter(string name, string value);

        IWorkItemQuery BuildWIQLQuery(IMigrationClient migrationClient);
    }
}