using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsChangeSetMappingToolOptions : ToolOptions
    {
        /// <summary>
        /// Path to changeset mapping file.
        /// </summary>
        /// <default>null</default>
        public string ChangeSetMappingFile { get; set; }
    }
}
