using MigrationTools._Enginev1.DataContracts;
using MigrationTools.Configuration;

namespace MigrationTools._Enginev1.Containers
{
    public interface IFieldMap
    {
        string Name { get; }
        string MappingDisplayName { get; }

        void Configure(IFieldMapConfig config);

        void Execute(WorkItemData source, WorkItemData target);
    }
}