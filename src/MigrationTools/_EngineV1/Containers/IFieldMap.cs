using System;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.DataContracts;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        [Obsolete]
        void Configure(IFieldMapOptions config);

        void Execute(WorkItemData source, WorkItemData target);
    }
}