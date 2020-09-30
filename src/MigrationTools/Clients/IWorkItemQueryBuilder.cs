using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Clients
{
   public interface IWorkItemQueryBuilder
    {
        Dictionary<string, string> Parameters { get; }
        string Query { get; set; }
        void AddParameter(string name, string value);

        IWorkItemQuery Build(IMigrationClient migrationClient);

    }
}
