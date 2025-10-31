namespace MigrationTools.Tools.Interfaces;

/// <summary>
/// Tool for exporting mappings of work item IDs from source to target.
/// </summary>
public interface IExportWorkItemMappingTool
{
    /// <summary>
    /// Add new work item mapping.
    /// </summary>
    /// <param name="sourceId">Source work item ID.</param>
    /// <param name="targetId">Target work item ID.</param>
    void AddMapping(string sourceId, string targetId);

    /// <summary>
    /// Save mappings.
    /// </summary>
    void SaveMappings();
}
