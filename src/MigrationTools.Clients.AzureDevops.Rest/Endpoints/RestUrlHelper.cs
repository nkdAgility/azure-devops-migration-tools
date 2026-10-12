using System;
using System.Linq;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Definition ids, field reference names and work item type names can contain reserved characters
    /// such as '#', '%', '?', '/' or spaces, so they must be escaped before going into a REST URL (#2611).
    /// </summary>
    public static class RestUrlHelper
    {
        public static string EscapeSegment(string value)
        {
            return value == null ? string.Empty : Uri.EscapeDataString(value);
        }

        public static string FormatRoute(string template, params string[] routeParameters)
        {
            var escaped = (routeParameters ?? new string[] { }).Select(EscapeSegment).ToArray<object>();
            return string.Format(template, escaped);
        }

        public static string AppendSegment(string baseUrl, string segment)
        {
            return baseUrl.TrimEnd('/') + "/" + EscapeSegment(segment);
        }
    }
}
