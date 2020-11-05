using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Configuration;

namespace MigrationTools._EngineV1.Containers
{
    public interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        void Configure(IFieldMapConfig config);

        void Execute(WorkItemData source, WorkItemData target);
    }
}