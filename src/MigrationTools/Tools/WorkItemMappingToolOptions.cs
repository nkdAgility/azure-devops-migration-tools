using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools;

/// <summary>
/// Options for <see cref="WorkItemMappingTool"/>.
/// </summary>
public class WorkItemMappingToolOptions : ToolOptions
{
    public WorkItemMappingToolOptions()
    {
        Enabled = false;
    }

    /// <summary>
    /// Path to file, wher work item mapping will be saved.
    /// </summary>
    public string TargetFile { get; set; }

    /// <summary>
    /// Indicates whether existing mappings in the target file should be preserved when saving new mappings.
    /// Default value is <see langword="true"/>.
    /// </summary>
    public bool PreserveExisting { get; set; } = true;
}
