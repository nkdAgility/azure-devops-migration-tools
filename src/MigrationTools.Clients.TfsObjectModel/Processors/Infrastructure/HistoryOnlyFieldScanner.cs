using System;
using System.Collections.Generic;
using MigrationTools.DataContracts;

namespace MigrationTools.Processors.Infrastructure
{
    /// <summary>
    /// Scans source revisions before migration for fields that carry a value but exist neither on the current
    /// source work item type nor on the target type (#3194). These history-only fields are not reported by the work
    /// item type validator, which inspects only current type definitions, and field maps cannot read them, because
    /// maps validate against the current source type.
    /// </summary>
    /// <remarks>
    /// A revision exposes the fields of the type it was saved under, so a field can only be history-only in a revision
    /// whose team project or work item type differs from the item's current one (the item was moved or its type was
    /// changed). Only those revisions are loaded, which keeps the scan cheap when few items have been moved.
    /// </remarks>
    public class HistoryOnlyFieldScanner
    {
        private const string TeamProjectField = "System.TeamProject";

        private readonly Func<string, string> _getTargetType;
        private readonly Func<string, ISet<string>> _getSourceTypeFields;
        private readonly Func<string, ISet<string>> _getTargetTypeFields;
        private readonly Func<WorkItemData, RevisionItem, IEnumerable<FieldItem>> _getRevisionFields;
        private readonly Func<string, string, bool> _isExcluded;

        /// <param name="getTargetType">Maps a source type name to the target type name (work item type mapping).</param>
        /// <param name="getSourceTypeFields">Fields of a current source type; null if the type is not validated.</param>
        /// <param name="getTargetTypeFields">Fields of a target type; null if the type does not exist in the target.</param>
        /// <param name="getRevisionFields">
        /// Loads the fields of a revision as saved under its own type. Defaults to <see cref="RevisionItem.Fields"/>.
        /// </param>
        /// <param name="isExcluded">Optional (sourceType, field) predicate; excluded fields go to <see cref="Excluded"/>.</param>
        public HistoryOnlyFieldScanner(
            Func<string, string> getTargetType,
            Func<string, ISet<string>> getSourceTypeFields,
            Func<string, ISet<string>> getTargetTypeFields,
            Func<WorkItemData, RevisionItem, IEnumerable<FieldItem>> getRevisionFields = null,
            Func<string, string, bool> isExcluded = null)
        {
            _getTargetType = getTargetType ?? throw new ArgumentNullException(nameof(getTargetType));
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
                if (workItem?.Revisions == null)
                {
                    continue;
                }
                // Replay writes every revision to the item's current type, which is also what field maps validate against.
                string sourceType = workItem.Type;
                ISet<string> sourceFields = _getSourceTypeFields(sourceType);
                if (sourceFields == null)
                {
                    continue;
                }
                string targetType = _getTargetType(sourceType);
                ISet<string> targetFields = _getTargetTypeFields(targetType);
                if (targetFields == null)
                {
                    // A missing target type is reported by the work item type validator.
                    continue;
                }
                foreach (var revision in workItem.Revisions.Values)
                {
                    if (revision == null || !IsMovedOrRetyped(workItem, revision))
                    {
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
