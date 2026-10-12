using System;
using System.Linq;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Helpers for building Azure DevOps REST URLs from values such as definition ids, field reference names
    /// and work item type names, which may contain reserved characters like '#', '%', '?', '/' or spaces (#2611).
    /// </summary>
    public static class RestUrlHelper
    {
        /// <summary>
        /// Escapes a value so it can be used as a single URL path segment or query string value.
        /// </summary>
        public static string EscapeSegment(string value)
        {
            return value == null ? string.Empty : Uri.EscapeDataString(value);
        }

        /// <summary>
        /// Formats a route template (e.g. "work/processes/{0}/workItemTypes/{1}/fields") with escaped route parameters.
        /// </summary>
        public static string FormatRoute(string template, params string[] routeParameters)
        {
            var escaped = (routeParameters ?? new string[] { }).Select(EscapeSegment).ToArray<object>();
            return string.Format(template, escaped);
        }

        /// <summary>
        /// Appends an escaped path segment to a base URL, without duplicating the separating slash.
        /// </summary>
        public static string AppendSegment(string baseUrl, string segment)
        {
            return baseUrl.TrimEnd('/') + "/" + EscapeSegment(segment);
        }
    }
}
