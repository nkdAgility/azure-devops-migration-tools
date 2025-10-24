using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools;

/// <summary>
/// Tool for exporting mappings of work item IDs from source to target.
/// Work item migration processor uses this tool to record work item ID mappings.
/// The mappings will be saved to file defined in options at the end of the migration.
/// </summary>
public class ExportWorkItemMappingTool : Tool<ExportWorkItemMappingToolOptions>, IExportWorkItemMappingTool
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly ConcurrentDictionary<string, string> _mappings = [];

    public ExportWorkItemMappingTool(
        IOptions<ExportWorkItemMappingToolOptions> options,
        IServiceProvider services,
        ILogger<ITool> logger,
        ITelemetryLogger telemetry)
        : base(options, services, logger, telemetry)
    {
    }

    /// <inheritdoc/>
    public void AddMapping(string sourceId, string targetId)
    {
        if (!Enabled)
        {
            return;
        }
        if (!_mappings.TryAdd(sourceId, targetId))
        {
            if (_mappings.TryGetValue(sourceId, out string existingTargetId)
                && !existingTargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase))
            {
                Log.LogError("Attempt to map source work item ID '{sourceId}' to target ID '{targetId}'."
                    + " This source work item ID is already mapped to different target ID '{existingTargetId}'."
                    + " Former mapping will be preserved.",
                    sourceId, targetId, existingTargetId);
            }
            return;
        }
        Log.LogDebug("Source work item ID '{sourceId}' is mapped to target work item ID '{targetId}'.", sourceId, targetId);
        _mappings[sourceId] = targetId;
    }

    /// <summary>
    /// Save mappings to file defined in options. Mappings is saved as dictionary serialized to JSON.
    /// </summary>
    public void SaveMappings()
    {
        if (!Enabled)
        {
            return;
        }

        if (string.IsNullOrEmpty(Options.TargetFile))
        {
            Log.LogError("Cannot save work item mappings. Path to target file ('TargetFile' option) is not set.");
        }
        else
        {
            Log.LogInformation("Saving work item mappings to file '{targetFile}'.", Options.TargetFile);
            Dictionary<string, string> allMappings = new(_mappings);
            if (Options.PreserveExisting)
            {
                Log.LogInformation($"'{nameof(Options.PreserveExisting)}' is set to 'true'."
                    + " Loading existing work item mappings from '{targetFile}'.", Options.TargetFile);
                Dictionary<string, string> existingMappings = LoadExistingMappings();
                allMappings = MergeWithExistingMappings(_mappings, existingMappings);
            }
            SaveMappingsCore(allMappings);
        }
    }

    private void SaveMappingsCore(Dictionary<string, string> mappings)
    {
        try
        {
            string tempFile = Options.TargetFile + ".tmp";
            using (FileStream target = File.Create(tempFile))
            {
                JsonSerializer.Serialize(target, mappings, _jsonOptions);
            }
            File.Copy(tempFile, Options.TargetFile, overwrite: true);
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to save work item mappings to '{targetFile}'.", Options.TargetFile);
        }
    }

    private Dictionary<string, string> LoadExistingMappings()
    {
        try
        {
            if (File.Exists(Options.TargetFile))
            {
                using Stream source = File.OpenRead(Options.TargetFile);
                Dictionary<string, string>? existing = JsonSerializer.Deserialize<Dictionary<string, string>>(source);
                return existing ?? [];
            }
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load existing work item mappings from '{targetFile}'.", Options.TargetFile);
        }
        return [];
    }

    private Dictionary<string, string> MergeWithExistingMappings(
        IDictionary<string, string> mappings,
        IDictionary<string, string> existingMappings)
    {
        Dictionary<string, string> result = new(mappings);

        foreach (KeyValuePair<string, string> existingMapping in existingMappings)
        {
            string sourceId = existingMapping.Key;
            string existingTargetId = existingMapping.Value;
            if (result.TryGetValue(sourceId, out string currentTargetId)
                && !currentTargetId.Equals(existingTargetId, StringComparison.OrdinalIgnoreCase))
            {
                Log.LogWarning("Current mapping for source work item ID '{sourceId}' is '{currentTargetId}'"
                    + " which is different from preserved target ID '{existingTargetId}'."
                    + " Preserved target ID will be discarded and current one will be used.",
                    sourceId, currentTargetId, existingTargetId);
                continue;
            }
            result[sourceId] = existingTargetId;
        }
        return result;
    }
}
