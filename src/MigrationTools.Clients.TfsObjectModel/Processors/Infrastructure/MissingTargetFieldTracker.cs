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
        private readonly int _maxSampleIds;

        public MissingTargetFieldTracker(int maxSampleIdsPerField = 20)
        {
            _maxSampleIds = maxSampleIdsPerField;
        }

        private sealed class Stats
        {
            // The processor replays each work item's revisions in ascending order, so counting against the last
            // item/revision seen keeps memory bounded and ignores a revision replayed by a retry.
            public int RevisionCount;
            public int WorkItemCount;
            public string LastWorkItemId;
            public int LastRevision = int.MinValue;
            // Only a bounded sample of IDs is kept for the summary.
            public readonly List<string> SampleWorkItemIds = new List<string>();
        }

        private readonly Dictionary<(string SourceType, string TargetType, string Field), Stats> _fields = new();

        public int Count => _fields.Count;

        /// <summary>Records an occurrence. Returns true the first time this field is seen for the type pair.</summary>
        public bool Record(string sourceType, string targetType, string fieldReferenceName, string sourceWorkItemId, int sourceRevision)
        {
            var key = (sourceType ?? string.Empty, targetType ?? string.Empty, fieldReferenceName ?? string.Empty);
            bool isNew = false;
            if (!_fields.TryGetValue(key, out var stats))
            {
                stats = new Stats();
                _fields[key] = stats;
                isNew = true;
            }
            var id = sourceWorkItemId ?? string.Empty;
            if (!string.Equals(stats.LastWorkItemId, id, StringComparison.Ordinal))
            {
                stats.LastWorkItemId = id;
                stats.LastRevision = int.MinValue;
                stats.WorkItemCount++;
                if (sourceWorkItemId != null && stats.SampleWorkItemIds.Count < _maxSampleIds)
                {
                    stats.SampleWorkItemIds.Add(sourceWorkItemId);
                }
            }
            if (sourceRevision > stats.LastRevision)
            {
                stats.LastRevision = sourceRevision;
                stats.RevisionCount++;
            }
            return isNew;
        }

        public IReadOnlyList<string> GetSummary()
        {
            return _fields
                .OrderBy(kv => kv.Key.TargetType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(kv => kv.Key.SourceType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(kv => kv.Key.Field, StringComparer.OrdinalIgnoreCase)
                .Select(kv => $"{kv.Key.Field} (source type '{kv.Key.SourceType}' -> target type '{kv.Key.TargetType}'): "
                    + $"{kv.Value.RevisionCount} revision(s) across {kv.Value.WorkItemCount} work item(s) "
                    + $"[{string.Join(", ", kv.Value.SampleWorkItemIds)}"
                    + $"{(kv.Value.WorkItemCount > kv.Value.SampleWorkItemIds.Count ? ", ..." : string.Empty)}]")
                .ToList();
        }
    }
}
