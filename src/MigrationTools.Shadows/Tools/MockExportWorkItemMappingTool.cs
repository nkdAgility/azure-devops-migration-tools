using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockExportWorkItemMappingTool : IExportWorkItemMappingTool
    {
        public void AddMapping(string sourceId, string targetId)
        {
        }

        public void SaveMappings()
        {
        }
    }
}
