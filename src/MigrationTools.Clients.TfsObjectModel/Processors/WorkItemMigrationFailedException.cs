using System;

namespace MigrationTools.Processors
{
    /// <summary>
    /// A work item could not be migrated. The message says what failed, why, and what to do, so it can be shown without a
    /// stack trace; the original exception is kept as the inner exception for Debug logging (#3270, #3275).
    /// </summary>
    public class WorkItemMigrationFailedException : Exception
    {
        public WorkItemMigrationFailedException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Builds the message for a failed revision. <paramref name="sourceRevision"/> is null when the failure happened
        /// before any revision was replayed (for example creating the target shell); <paramref name="targetId"/> is null
        /// when the target does not exist yet.
        /// </summary>
        public static string Describe(string sourceId, int? sourceRevision, string targetId, string workItemType, Exception ex)
        {
            var reason = ServerReason(ex);
            var what = $"Source work item {sourceId}"
                + (sourceRevision.HasValue ? $" revision {sourceRevision}" : "")
                + $" ({workItemType ?? "unknown type"})"
                + (!string.IsNullOrEmpty(targetId) && targetId != "0" ? $" could not be saved to target work item {targetId}." : " could not be saved to the target.");
            return $"{what} Reason: {reason} What to do: {Advice(reason, workItemType)}";
        }

        /// <summary>The innermost exception message, on one line: the reason the target or the object model gave.</summary>
        public static string ServerReason(Exception ex)
        {
            if (ex == null) return "unknown.";
            if (ex is WorkItemMigrationFailedException) return ex.Message;
            var message = ex.GetBaseException().Message ?? ex.Message ?? "";
            return System.Text.RegularExpressions.Regex.Replace(message, @"\s+", " ").Trim();
        }

        /// <summary>What the user can do about a known Azure DevOps error code in <paramref name="reason"/>.</summary>
        public static string Advice(string reason, string workItemType)
        {
            reason = reason ?? "";
            if (reason.Contains("TF26194"))
                return "the target refused a field value that the object model treats as read-only. Give the migration identity 'Bypass rules on work item updates' in the target project, or map the field to a writable one with a field map.";
            if (reason.Contains("VS403417"))
                return $"the target would not create or update a '{workItemType}' with these values. Check that the work item type '{workItemType}' exists and is enabled in the target process, or map it to another type with WorkItemTypeMappingTool.";
            if (reason.Contains("TF237124"))
                return "some fields are not valid on the target. Run with Serilog MinimumLevel 'Debug' to list them, then fix the process or add field maps.";
            if (reason.Contains("TF237111") || reason.Contains("TF237165") || reason.Contains("TF401289") || reason.IndexOf("permission", StringComparison.OrdinalIgnoreCase) >= 0)
                return "grant the migration identity permission to edit work items in the target area and iteration paths, and 'Bypass rules on work item updates'.";
            if (reason.Contains("already migrated target work item does not match"))
                return "open the target work item's history and compare the listed revisions with the source. Where an identity differs, check the user mapping names the account you expect: a migration replays the mapping, so deleting the item and migrating it again gives the same values until the mapping is right. Where the target holds the service's own date or the migration identity, give the migration identity 'Bypass rules on work item updates' in the target project, check no target process rule sets these fields, then delete the target work item and migrate it again: saved revisions are not corrected after the fact.";
            if (reason.Contains("state-stamped") || reason.Contains("stamped instead of the source"))
                return "give the migration identity 'Bypass rules on work item updates' in the target project and check no target process rule sets these fields. Then delete the target work item and migrate it again: saved revisions are not corrected after the fact, and every rerun reports this item until it is migrated again.";
            return "check the reason above against the target project and process. Run with Serilog MinimumLevel 'Debug' for the full details.";
        }
    }
}
