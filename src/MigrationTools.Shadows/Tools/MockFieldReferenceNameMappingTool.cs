using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockFieldReferenceNameMappingTool : IFieldReferenceNameMappingTool
    {
        public string? GetTargetFieldName(string workItemType, string sourceFieldName, out bool isMapped)
        {
            isMapped = false;
            return sourceFieldName;
        }
    }
}
