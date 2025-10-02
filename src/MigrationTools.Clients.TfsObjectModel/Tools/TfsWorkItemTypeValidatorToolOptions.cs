using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsWorkItemTypeValidatorToolOptions : ToolOptions
    {
        /// <summary>
        /// Special key, meaning this mapping applied to all work item types.
        /// </summary>
        public const string AllWorkItemTypes = "*";

        private static readonly StringComparer _normalizedComparer = StringComparer.OrdinalIgnoreCase;
        private bool _isNormalized = false;
        private static readonly string[] _defaultExcludedWorkItemTypes = [
            "Code Review Request",
            "Code Review Response",
            "Feedback Request",
            "Feedback Response",
            "Shared Parameter",
            "Shared Steps"
        ];

        /// <summary>
        /// List of work item types which will be validated. If this list is empty, all work item types will be validated.
        /// </summary>
        /// <default>null</default>
        public List<string> IncludeWorkItemTypes { get; set; } = [];

        /// <summary>
        /// List of work item types which will be excluded from validation.
        /// </summary>
        public List<string> ExcludeWorkItemTypes { get; set; } = [];

        /// <summary>
        /// If <see langword="true"/>, some work item types will be automatically added to <see cref="ExcludeWorkItemTypes"/> list.
        /// Work item types excluded by default are: Code Review Request, Code Review Response, Feedback Request,
        /// Feedback Response, Shared Parameter, Shared Steps.
        /// </summary>
        public bool ExcludeDefaultWorkItemTypes { get; set; } = true;

        /// <summary>
        /// <para>
        /// List of fields in source work itemt types, that are excluded from validation.
        /// Fields excluded from validation are still validated and all found issues are logged.
        /// But the result of the validation is 'valid' and the issues are logged as information instead of warning.
        /// </para>
        /// <para>
        /// The key is the source work item type name.
        /// You can also use <c>*</c> to exclude fields from all source work item types.
        /// </para>
        /// </summary>
        /// <default>null</default>
        public Dictionary<string, List<string>> ExcludeSourceFields { get; set; } = [];

        /// <summary>
        /// Normalizes properties, that all of them are set (not <see langword="null"/>) and all dictionaries uses
        /// case-insensitive keys.
        /// </summary>
        public void Normalize()
        {
            if (_isNormalized)
            {
                return;
            }

            Dictionary<string, List<string>> oldExcludedFields = ExcludeSourceFields;
            Dictionary<string, List<string>> newExcludedFields = new(_normalizedComparer);
            if (oldExcludedFields is not null)
            {
                foreach (KeyValuePair<string, List<string>> mapping in oldExcludedFields)
                {
                    newExcludedFields[mapping.Key.Trim()] = mapping.Value;
                }
            }

            IncludeWorkItemTypes ??= [];
            ExcludeWorkItemTypes ??= [];
            if (ExcludeDefaultWorkItemTypes)
            {
                foreach (string defaultExcludedWit in _defaultExcludedWorkItemTypes)
                {
                    if (!ExcludeWorkItemTypes.Contains(defaultExcludedWit, _normalizedComparer))
                    {
                        ExcludeWorkItemTypes.Add(defaultExcludedWit);
                    }
                }
            }

            ExcludeSourceFields = newExcludedFields;
            _isNormalized = true;
        }

        /// <summary>
        /// Returns true, if field <paramref name="fieldReferenceName"/> from work item type <paramref name="workItemType"/>
        /// is in list of fixed target fields. Handles also fields defined for all work item types (<c>*</c>).
        /// </summary>
        /// <param name="workItemType">Work item type name.</param>
        /// <param name="fieldReferenceName">Target field reference name.</param>
        public bool IsSourceFieldExcluded(string workItemType, string fieldReferenceName)
        {
            if (ExcludeSourceFields.TryGetValue(workItemType, out List<string> excludedFields))
            {
                return excludedFields.Contains(fieldReferenceName, _normalizedComparer);
            }
            if (ExcludeSourceFields.TryGetValue(AllWorkItemTypes, out excludedFields))
            {
                return excludedFields.Contains(fieldReferenceName, _normalizedComparer);
            }
            return false;
        }
    }
}
