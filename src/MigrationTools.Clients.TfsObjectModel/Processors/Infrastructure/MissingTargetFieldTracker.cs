using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors.Infrastructure
{
    /// <summary>
    /// Tracks source fields that carried a value in a migrated revision but do not exist on the target work item type.
    /// The work item type validator only inspects the current source type definitions, so fields that only appear
    /// in historical revisions (e.g. items moved from another project or process) are not caught up front (#3173).
    /// </summary>
    public class MissingTargetFieldTracker
    {
        private sealed class Stats
        {
            public int RevisionCount;
            public readonly SortedSet<string> SourceWorkItemIds = new SortedSet<string>(StringComparer.Ordinal);
        }

        private readonly Dictionary<(string SourceType, string TargetType, string Field), Stats> _fields = new();

        public int Count => _fields.Count;

        /// <summary>Records an occurrence. Returns true the first time this field is seen for the type pair.</summary>
        public bool Record(string sourceType, string targetType, string fieldReferenceName, string sourceWorkItemId)
        {
            var key = (sourceType ?? string.Empty, targetType ?? string.Empty, fieldReferenceName ?? string.Empty);
            bool isNew = false;
            if (!_fields.TryGetValue(key, out var stats))
            {
                stats = new Stats();
                _fields[key] = stats;
                isNew = true;
            }
            stats.RevisionCount++;
            if (sourceWorkItemId != null)
            {
                stats.SourceWorkItemIds.Add(sourceWorkItemId);
            }
            return isNew;
        }

        public IReadOnlyList<string> GetSummary(int maxIdsPerField = 20)
        {
            return _fields
                .OrderBy(kv => kv.Key.TargetType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(kv => kv.Key.SourceType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(kv => kv.Key.Field, StringComparer.OrdinalIgnoreCase)
                .Select(kv => $"{kv.Key.Field} (source type '{kv.Key.SourceType}' -> target type '{kv.Key.TargetType}'): "
                    + $"{kv.Value.RevisionCount} revision(s) across {kv.Value.SourceWorkItemIds.Count} work item(s) "
                    + $"[{string.Join(", ", kv.Value.SourceWorkItemIds.Take(maxIdsPerField))}"
                    + $"{(kv.Value.SourceWorkItemIds.Count > maxIdsPerField ? ", ..." : string.Empty)}]")
                .ToList();
        }
    }
}
