using MigrationTools.DataContracts;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockFieldMappingTool : IFieldMappingTool
    {
        public Dictionary<string, List<IFieldMap>> Items => throw new NotImplementedException();

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            throw new NotImplementedException();
        }

        public List<IFieldMap> GetFieldMappings(string witName) => [];

        public void ApplyFieldMappings(WorkItemData source, WorkItemData target)
        {
            throw new NotImplementedException();
        }

        public void ApplyFieldMappings(WorkItemData target)
        {
            throw new NotImplementedException();
        }
    }
}
