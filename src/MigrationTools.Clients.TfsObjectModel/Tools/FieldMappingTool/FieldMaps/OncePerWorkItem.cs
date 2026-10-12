using System;
using System.Collections.Generic;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel;

#nullable enable

/// <summary>
/// Lets a field map log a problem once per work item instead of once per replayed revision. Work items are replayed one
/// after another, so only the keys of the current work item are kept.
/// </summary>
internal sealed class OncePerWorkItem
{
    private string? _workItemId;
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);

    public bool IsFirst(string workItemId, string key)
    {
        if (!string.Equals(_workItemId, workItemId, StringComparison.Ordinal))
        {
            _workItemId = workItemId;
            _keys.Clear();
        }
        return _keys.Add(key);
    }
}
