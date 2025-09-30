using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools.Interfaces
{
    public interface IFieldMappingTool
    {
        Dictionary<string, List<IFieldMap>> Items { get; }

        void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap);
        List<IFieldMap> GetFieldMappings(string witName);
        void ApplyFieldMappings(WorkItemData source, WorkItemData target);
        void ApplyFieldMappings(WorkItemData target);
    }
}
