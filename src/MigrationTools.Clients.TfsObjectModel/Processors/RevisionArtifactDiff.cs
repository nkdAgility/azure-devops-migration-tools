using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Pure logic for replaying attachments and links at the revision where they changed in the source (#3192).
    /// Compares the source state of one revision with the state of the previous replayed revision.
    /// </summary>
    public static class RevisionArtifactDiff
    {
        /// <summary>
        /// Returns the items present in <paramref name="current"/> but not in <paramref name="previous"/> (Added),
        /// and the items present in <paramref name="previous"/> but not in <paramref name="current"/> (Removed).
        /// A null <paramref name="previous"/> means nothing was replayed before, so nothing is removed.
        /// Items are compared by key; duplicate keys are treated as one item.
        /// </summary>
        public static (List<T> Added, List<T> Removed) Compare<T>(IEnumerable<T> previous, IEnumerable<T> current, Func<T, string> key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            var prev = Distinct(previous, key);
            var curr = Distinct(current, key);
            var added = curr.Where(kv => !prev.ContainsKey(kv.Key)).Select(kv => kv.Value).ToList();
            var removed = previous == null
                ? new List<T>()
                : prev.Where(kv => !curr.ContainsKey(kv.Key)).Select(kv => kv.Value).ToList();
            return (added, removed);
        }

        /// <summary>Key for a hyperlink, compared case-insensitively.</summary>
        public static string HyperlinkKey(string location) => "H|" + (location ?? string.Empty).Trim('"').ToLowerInvariant();

        /// <summary>Key for an external (artifact) link, compared case-insensitively.</summary>
        public static string ExternalLinkKey(string linkedArtifactUri) => "E|" + (linkedArtifactUri ?? string.Empty).ToLowerInvariant();

        /// <summary>Key for a work item link: link type end plus the related work item id.</summary>
        public static string RelatedLinkKey(string linkTypeEndImmutableName, int relatedWorkItemId) => "R|" + linkTypeEndImmutableName + "|" + relatedWorkItemId;

        /// <summary>Key for an attachment: the source file id.</summary>
        public static string AttachmentKey(int fileId) => "A|" + fileId;

        /// <summary>The marker written to a migrated attachment's comment to identify its source file id.</summary>
        public static string AttachmentOriginalIdMarker(int fileId) => "[originalId:" + fileId + "]";

        private static Dictionary<string, T> Distinct<T>(IEnumerable<T> items, Func<T, string> key)
        {
            var result = new Dictionary<string, T>(StringComparer.Ordinal);
            if (items == null) return result;
            foreach (var item in items)
            {
                var k = key(item);
                if (!result.ContainsKey(k)) result.Add(k, item);
            }
            return result;
        }
    }
}
