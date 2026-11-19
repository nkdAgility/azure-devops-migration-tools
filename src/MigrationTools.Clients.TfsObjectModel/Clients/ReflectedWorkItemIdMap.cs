using System;
using System.Collections.Generic;

namespace MigrationTools.Clients
{
    /// <summary>
    /// Source work item → target work item ids, read once from the reflected work item ID of every target work item (#3299),
    /// so a re-run finds already migrated items and the targets of their links without a WIQL query per item.
    /// </summary>
    /// <remarks>
    /// Keyed by source collection and work item id, not project: ids are unique in a collection and a work item can move project.
    /// A source item reflected by more than one target item is ambiguous and is never answered from the map.
    /// </remarks>
    public class ReflectedWorkItemIdMap
    {
        private readonly Dictionary<string, int> _targetIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int Count => _targetIds.Count;

        public int AmbiguousCount => _ambiguous.Count;

        /// <summary>Records a target item; values that are not a reflected work item ID are ignored.</summary>
        public void Add(string reflectedWorkItemId, int targetId)
        {
            var key = KeyOf(reflectedWorkItemId);
            if (key is null)
            {
                return;
            }
            if (_ambiguous.Contains(key))
            {
                return;
            }
            if (_targetIds.TryGetValue(key, out var existing) && existing != targetId)
            {
                _targetIds.Remove(key);
                _ambiguous.Add(key);
                return;
            }
            _targetIds[key] = targetId;
        }

        public void Add(Uri sourceCollection, int sourceId, int targetId)
        {
            Add(Format(sourceCollection, sourceId), targetId);
        }

        public bool TryGetTargetId(string reflectedWorkItemId, out int targetId)
        {
            targetId = 0;
            var key = KeyOf(reflectedWorkItemId);
            return key is not null && _targetIds.TryGetValue(key, out targetId);
        }

        public bool TryGetTargetId(Uri sourceCollection, int sourceId, out int targetId)
        {
            return TryGetTargetId(Format(sourceCollection, sourceId), out targetId);
        }

        /// <summary>True when the map cannot say whether the item is in the target, so the caller must query for it.</summary>
        public bool IsAmbiguous(string reflectedWorkItemId)
        {
            var key = KeyOf(reflectedWorkItemId);
            return key is null || _ambiguous.Contains(key);
        }

        private static string Format(Uri collection, int id) => $"{collection.ToString().TrimEnd('/')}/_/_workitems/edit/{id}";

        private static string KeyOf(string reflectedWorkItemId)
        {
            if (!TfsReflectedWorkItemId.IsReflectedWorkItemId(reflectedWorkItemId))
            {
                return null;
            }
            var marker = reflectedWorkItemId.LastIndexOf("/_workitems/edit/", StringComparison.OrdinalIgnoreCase);
            var idPart = reflectedWorkItemId.Substring(marker + "/_workitems/edit/".Length).Trim();
            var projectSeparator = reflectedWorkItemId.LastIndexOf('/', marker - 1);
            if (projectSeparator <= 0 || !int.TryParse(idPart, out var id))
            {
                return null;
            }
            var collection = reflectedWorkItemId.Substring(0, projectSeparator).TrimEnd('/');
            return collection + "|" + id;
        }
    }
}
