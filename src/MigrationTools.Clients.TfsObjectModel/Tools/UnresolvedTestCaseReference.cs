using Microsoft.Extensions.Logging;
using MigrationTools.Services;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Reports a Test Case reference to a Shared Steps or Shared Parameter that has no match in the target, so the
    /// reference keeps its source id.
    /// </summary>
    public static class UnresolvedTestCaseReference
    {
        /// <summary>
        /// Logs a warning and records a <see cref="WorkItemReviewCategory.ReferenceLeftOnSource"/> review entry.
        /// </summary>
        /// <param name="kind">"Shared Steps" or "Shared Parameter".</param>
        /// <param name="field">The Test Case field that holds the reference.</param>
        public static void Report(ILogger logger, WorkItemReviewLog reviewLog, string kind, string field, string referencedSourceId, string testCaseSourceId, string testCaseTargetId)
        {
            logger?.LogWarning("  [SKIP] {Kind} {RelatedId} used by Test Case {SourceId} (target {TargetId}) not remapped: it has not been migrated to the target, so {Field} still holds the source id {RelatedId}. Migrate the {Kind}, then re-run.",
                kind, referencedSourceId, testCaseSourceId, testCaseTargetId, field, referencedSourceId, kind);
            reviewLog?.Record(WorkItemReviewCategory.ReferenceLeftOnSource,
                $"{kind} {referencedSourceId} used by this Test Case has not been migrated to the target, so {field} still holds the source id {referencedSourceId}.",
                $"Migrate the {kind} (include '{kind}' in the WIQL query), then re-run to remap the reference.",
                sourceId: testCaseSourceId, targetId: testCaseTargetId);
        }
    }
}
