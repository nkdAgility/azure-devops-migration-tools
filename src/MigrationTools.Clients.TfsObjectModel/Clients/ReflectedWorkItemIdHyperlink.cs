using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace MigrationTools.Clients
{
    /// <summary>
    /// Records the reflected work item ID on a target work item whose type cannot hold the reflected work item ID
    /// field, such as Shared Steps and Shared Parameter on an inherited process. The ID is kept as a hyperlink to the
    /// source work item, marked by <see cref="Comment"/>, so a later lookup can find the item again.
    /// </summary>
    public static class ReflectedWorkItemIdHyperlink
    {
        public const string Comment = "Migrated from (ReflectedWorkItemId)";

        public static bool IsMarker(string comment) => string.Equals(comment, Comment, StringComparison.Ordinal);

        /// <summary>
        /// The hyperlink location for a reflected work item ID. Spaces in a project name are escaped so the
        /// location is a valid URL.
        /// </summary>
        public static string ToLocation(string reflectedWorkItemId) => Uri.EscapeUriString(Uri.UnescapeDataString(reflectedWorkItemId));

        /// <summary>
        /// A comparison key that treats an escaped location and the plain reflected ID as the same.
        /// </summary>
        public static string ToKey(string reflectedWorkItemIdOrLocation)
            => Uri.UnescapeDataString(reflectedWorkItemIdOrLocation ?? string.Empty).Trim().TrimEnd('/').ToLowerInvariant();

        /// <summary>
        /// The reflected work item ID held by the marker hyperlink, or <see langword="null"/> if there is none.
        /// </summary>
        public static string Find(WorkItem workItem)
        {
            var location = workItem.Links.OfType<Hyperlink>().FirstOrDefault(h => IsMarker(h.Comment))?.Location;
            return location is null ? null : Uri.UnescapeDataString(location);
        }

        /// <summary>
        /// Adds the marker hyperlink if the work item does not already have one for this ID.
        /// Returns <see langword="true"/> if a link was added.
        /// </summary>
        public static bool Ensure(WorkItem workItem, string reflectedWorkItemId)
        {
            var key = ToKey(reflectedWorkItemId);
            if (workItem.Links.OfType<Hyperlink>().Any(h => IsMarker(h.Comment) && ToKey(h.Location) == key))
            {
                return false;
            }
            workItem.Links.Add(new Hyperlink(ToLocation(reflectedWorkItemId)) { Comment = Comment });
            return true;
        }
    }
}
