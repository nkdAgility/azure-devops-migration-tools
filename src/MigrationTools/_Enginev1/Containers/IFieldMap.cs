using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Engine.Containers
{
    public interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        void Configure(IFieldMapConfig config);

        void Execute(WorkItemData source, WorkItemData target);
    }
}