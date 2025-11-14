using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools;

/// <summary>
/// Options for <see cref="ExportWorkItemMappingTool"/>.
/// </summary>
public class ExportWorkItemMappingToolOptions : ToolOptions
{
    /// <summary>
    /// Path to file, where work item mapping will be saved.
    /// </summary>
    /// <default></default>
    public string TargetFile { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether existing mappings in the target file should be preserved when saving new mappings.
    /// Default value is <see langword="true"/>.
    /// </summary>
    /// <default>true</default>
    public bool PreserveExisting { get; set; } = true;
}
