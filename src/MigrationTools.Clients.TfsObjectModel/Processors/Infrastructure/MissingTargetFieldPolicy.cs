using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Processors.Infrastructure
{
    /// <summary>
    /// Decides whether a source value for a field that does not exist on the target work item type is dropped during
    /// revision replay, or is consumed by a field map (#3173, #3194).
    /// </summary>
    public class MissingTargetFieldPolicy
    {
        private readonly Func<string, IEnumerable<string>> _loadSourceTypeFields;
        private readonly IFieldMappingTool _fieldMappingTool;
        private readonly Dictionary<string, HashSet<string>> _sourceTypeFields = new(StringComparer.OrdinalIgnoreCase);

        /// <param name="loadSourceTypeFields">
        /// Field reference names of a work item type in the current source project; empty if the type is not there.
        /// Called once per type.
        /// </param>
        public MissingTargetFieldPolicy(Func<string, IEnumerable<string>> loadSourceTypeFields, IFieldMappingTool fieldMappingTool)
        {
            _loadSourceTypeFields = loadSourceTypeFields ?? throw new ArgumentNullException(nameof(loadSourceTypeFields));
            _fieldMappingTool = fieldMappingTool ?? throw new ArgumentNullException(nameof(fieldMappingTool));
        }

        /// <summary>
        /// Revisions of an item moved between projects report the old project's type, so the field is checked against
        /// the type in the current source project, which is what field maps validate against.
        /// </summary>
        public bool IsFieldOnCurrentSourceType(string sourceType, string fieldReferenceName)
        {
            if (!_sourceTypeFields.TryGetValue(sourceType, out var fields))
            {
                fields = new HashSet<string>(_loadSourceTypeFields(sourceType) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                _sourceTypeFields[sourceType] = fields;
            }
            return fields.Contains(fieldReferenceName);
        }

        public bool IsSourceFieldHandledByFieldMap(string sourceType, string fieldReferenceName)
        {
            return _fieldMappingTool.GetFieldToFieldMaps(sourceType, fieldReferenceName, FieldMapMode.SourceToTarget).Any()
                || _fieldMappingTool.GetFieldToFieldMultiMaps(sourceType, fieldReferenceName).Any();
        }

        /// <summary>
        /// A field map only reads a source field that is on the current source type, so a history-only field is dropped
        /// even when a map names it.
        /// </summary>
        public bool IsDropped(string sourceType, string fieldReferenceName)
        {
            return !(IsFieldOnCurrentSourceType(sourceType, fieldReferenceName)
                && IsSourceFieldHandledByFieldMap(sourceType, fieldReferenceName));
        }
    }
}
