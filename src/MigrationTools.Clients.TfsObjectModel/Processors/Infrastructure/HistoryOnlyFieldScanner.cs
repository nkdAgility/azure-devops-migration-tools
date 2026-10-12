using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;

namespace MigrationTools.Processors.Infrastructure
{
    /// <summary>
    /// Scans source revisions before migration for fields that carry a value but exist neither on the source work
    /// item type in the current project nor on the target type the revision is replayed to (#3194). These
    /// history-only fields are not reported by the work item type validator, which inspects only current type
    /// definitions, and field maps cannot read them, because maps validate against the current source type.
    /// </summary>
    /// <remarks>
    /// Revision replay loads each revision as saved under its own team project and work item type, and that exposes
    /// the fields of that type in that project. A revision saved under the item's current project and type therefore
    /// only exposes fields of the current type, so a field can only be history-only in a revision whose team project or
    /// type differs (the item was moved or retyped). Only those revisions are loaded, one server request each.
    /// </remarks>
    public class HistoryOnlyFieldScanner
    {
        private const string TeamProjectField = "System.TeamProject";

        private readonly IDictionary<string, string> _typeMappings;
        private readonly Func<string, bool> _isUsableInTarget;
        private readonly Func<string, ISet<string>> _getSourceTypeFields;
        private readonly Func<string, ISet<string>> _getTargetTypeFields;
        private readonly Func<WorkItemData, RevisionItem, IEnumerable<FieldItem>> _getRevisionFields;
        private readonly Func<string, string, bool> _isExcluded;

        /// <param name="isUsableInTarget">Whether a target type exists and is enabled, as used by revision replay.</param>
        /// <param name="getSourceTypeFields">
        /// Fields of a source type in the current project: null if the type is not validated, empty if it does not exist.
        /// </param>
        /// <param name="getTargetTypeFields">Fields of a target type; null if the type does not exist in the target.</param>
        /// <param name="getRevisionFields">
        /// Loads the fields of a revision as saved under its own type. Defaults to <see cref="RevisionItem.Fields"/>.
        /// </param>
        /// <param name="isExcluded">Optional (sourceType, field) predicate; excluded fields go to <see cref="Excluded"/>.</param>
        public HistoryOnlyFieldScanner(
            IDictionary<string, string> typeMappings,
            Func<string, bool> isUsableInTarget,
            Func<string, ISet<string>> getSourceTypeFields,
            Func<string, ISet<string>> getTargetTypeFields,
            Func<WorkItemData, RevisionItem, IEnumerable<FieldItem>> getRevisionFields = null,
            Func<string, string, bool> isExcluded = null)
        {
            _typeMappings = typeMappings ?? new Dictionary<string, string>();
            _isUsableInTarget = isUsableInTarget ?? throw new ArgumentNullException(nameof(isUsableInTarget));
            _getSourceTypeFields = getSourceTypeFields ?? throw new ArgumentNullException(nameof(getSourceTypeFields));
            _getTargetTypeFields = getTargetTypeFields ?? throw new ArgumentNullException(nameof(getTargetTypeFields));
            _getRevisionFields = getRevisionFields ?? ((_, revision) => revision.Fields?.Values);
            _isExcluded = isExcluded ?? ((_, _) => false);
        }

        /// <summary>History-only fields found by the last <see cref="Scan"/> that are excluded from validation.</summary>
        public MissingTargetFieldTracker Excluded { get; private set; } = new MissingTargetFieldTracker();

        /// <summary>Number of revisions loaded by the last <see cref="Scan"/> because they were moved or retyped.</summary>
        public int RevisionsLoaded { get; private set; }

        public MissingTargetFieldTracker Scan(IEnumerable<WorkItemData> workItems)
        {
            var tracker = new MissingTargetFieldTracker();
            Excluded = new MissingTargetFieldTracker();
            RevisionsLoaded = 0;
            foreach (var workItem in workItems)
            {
                if (workItem?.Revisions == null || workItem.Revisions.Count == 0
                    || _getSourceTypeFields(workItem.Type) == null)
                {
                    continue;
                }
                // Follow the same target type selection as revision replay: the shell type, then per revision.
                var revisions = workItem.Revisions.Values.Where(r => r != null).ToList();
                string targetType = WorkItemShellTypeSelector.Select(
                    revisions[0].Type ?? workItem.Type, workItem.Type, _typeMappings, _isUsableInTarget).ShellType;
                foreach (var revision in revisions)
                {
                    string sourceType = revision.Type ?? workItem.Type;
                    targetType = WorkItemShellTypeSelector.SelectRevisionType(sourceType, targetType, _typeMappings, _isUsableInTarget);
                    if (!IsMovedOrRetyped(workItem, revision))
                    {
                        continue;
                    }
                    ISet<string> sourceFields = _getSourceTypeFields(sourceType);
                    ISet<string> targetFields = _getTargetTypeFields(targetType);
                    if (sourceFields == null || targetFields == null)
                    {
                        // Not validated, or a missing target type, which the work item type validator reports.
                        continue;
                    }
                    RevisionsLoaded++;
                    IEnumerable<FieldItem> fields = _getRevisionFields(workItem, revision);
                    if (fields == null)
                    {
                        continue;
                    }
                    foreach (var field in fields)
                    {
                        if (sourceFields.Contains(field.ReferenceName) || targetFields.Contains(field.ReferenceName))
                        {
                            continue;
                        }
                        if (field.Value == null || string.Empty.Equals(field.Value))
                        {
                            continue;
                        }
                        var tracked = _isExcluded(sourceType, field.ReferenceName) ? Excluded : tracker;
                        tracked.Record(sourceType, targetType, field.ReferenceName, workItem.Id, revision.Number);
                    }
                }
            }
            return tracker;
        }

        private static bool IsMovedOrRetyped(WorkItemData workItem, RevisionItem revision)
        {
            if (!string.Equals(revision.Type, workItem.Type, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (revision.Fields != null && revision.Fields.TryGetValue(TeamProjectField, out FieldItem project)
                && project?.Value is string projectName && !string.IsNullOrEmpty(workItem.ProjectName))
            {
                return !string.Equals(projectName, workItem.ProjectName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
