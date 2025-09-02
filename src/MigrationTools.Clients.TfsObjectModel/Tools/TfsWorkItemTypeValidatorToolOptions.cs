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
        private static string[] _defaultExcludedWorkItemTypes = [
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
        public List<string> IncludeWorkItemtypes { get; set; } = [];

        /// <summary>
        /// List of work item types which will be excluded from validation.
        /// </summary>
        public List<string> ExcludeWorkItemtypes { get; set; } = [];

        /// <summary>
        /// If <see langword="true"/>, some work item types will be automatically added to <see cref="ExcludeWorkItemtypes"/> list.
        /// Work item types excluded by default are: Code Review Request, Code Review Response, Feedback Request,
        /// Feedback Response, Shared Parameter, Shared Steps.
        /// </summary>
        public bool ExcludeDefaultWorkItemTypes { get; set; } = true;

        /// <summary>
        /// Field reference name mappings. Key is work item type name, value is dictionary of mapping source filed name to
        /// target field name. Target field name can be empty string to indicate that this field will not be validated in target.
        /// As work item type name, you can use <c>*</c> to define mappings which will be applied to all work item types.
        /// </summary>
        /// <default>null</default>
        public Dictionary<string, Dictionary<string, string>> SourceFieldMappings { get; set; } = [];

        /// <summary>
        /// <para>
        /// List of target fields that are considered as <c>fixed</c>.
        /// A field marked as fixed will not stop the migration if differences are found.
        /// Instead of a warning, only an informational message will be logged.
        /// </para>
        /// <para>
        /// Use this list when you already know about the differences and have resolved them,
        /// for example by using <see cref="FieldMappingTool"/>.
        /// </para>
        /// <para>
        /// The key is the target work item type name.
        /// You can also use <c>*</c> to define fixed fields that apply to all work item types.
        /// </para>
        /// </summary>
        /// <default>null</default>
        public Dictionary<string, List<string>> FixedTargetFields { get; set; } = [];

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

            Dictionary<string, Dictionary<string, string>> oldMappings = SourceFieldMappings;
            Dictionary<string, Dictionary<string, string>> newMappings = new(_normalizedComparer);
            if (oldMappings is not null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> mapping in oldMappings)
                {
                    Dictionary<string, string> normalizedValues = new(_normalizedComparer);
                    foreach (KeyValuePair<string, string> fieldMapping in mapping.Value)
                    {
                        normalizedValues[fieldMapping.Key.Trim()] = fieldMapping.Value.Trim();
                    }
                    newMappings[mapping.Key.Trim()] = normalizedValues;
                }
            }

            Dictionary<string, List<string>> oldFixedFields = FixedTargetFields;
            Dictionary<string, List<string>> newFixedFields = new(_normalizedComparer);
            if (oldFixedFields is not null)
            {
                foreach (KeyValuePair<string, List<string>> mapping in oldFixedFields)
                {
                    newFixedFields[mapping.Key.Trim()] = mapping.Value;
                }
            }

            IncludeWorkItemtypes ??= [];
            ExcludeWorkItemtypes ??= [];
            if (ExcludeDefaultWorkItemTypes)
            {
                foreach (string defaultExcludedWit in _defaultExcludedWorkItemTypes)
                {
                    if (!ExcludeWorkItemtypes.Contains(defaultExcludedWit, _normalizedComparer))
                    {
                        ExcludeWorkItemtypes.Add(defaultExcludedWit);
                    }
                }
            }

            FixedTargetFields = newFixedFields;
            SourceFieldMappings = newMappings;
            _isNormalized = true;
        }

        /// <summary>
        /// Returns true, if field <paramref name="targetFieldName"/> from work item type <paramref name="workItemType"/>
        /// is in list of fixed target fields. Handles also fields defined for all work item types (<c>*</c>).
        /// </summary>
        /// <param name="workItemType">Work item type name.</param>
        /// <param name="targetFieldName">Target field reference name.</param>
        public bool IsFieldFixed(string workItemType, string targetFieldName)
        {
            if (FixedTargetFields.TryGetValue(workItemType, out List<string> fixedFields))
            {
                return fixedFields.Contains(targetFieldName, _normalizedComparer);
            }
            if (FixedTargetFields.TryGetValue(AllWorkItemTypes, out fixedFields))
            {
                return fixedFields.Contains(targetFieldName, _normalizedComparer);
            }
            return false;
        }

        /// <summary>
        /// Search for mapped target field name for given <paramref name="sourceFieldName"/>. If there is no mapping for source
        /// field name, its value is returned as target field name.
        /// Handles also mappings defined for all work item types (<c>*</c>).
        /// </summary>
        /// <param name="workItemType">Work item type name.</param>
        /// <param name="sourceFieldName">Source field reference name.</param>
        /// <param name="isMapped">Flag if returned value was mapped, or just returned the original.</param>
        /// <returns>
        /// Returns:
        /// <list type="bullet">
        /// <item>Target field name if it is foung in mappings. This can be empty string, which means that the source
        /// field is not mapped.</item>
        /// <item>Source filed name <paramref name="sourceFieldName"/> if there is no mapping defined for this field.</item>
        /// </list>
        /// </returns>
        public string GetTargetFieldName(string workItemType, string sourceFieldName, out bool isMapped)
        {
            if (TryGetTargetFieldName(workItemType, sourceFieldName, out string targetFieldName))
            {
                isMapped = true;
                return targetFieldName;
            }
            if (TryGetTargetFieldName(AllWorkItemTypes, sourceFieldName, out targetFieldName))
            {
                isMapped = true;
                return targetFieldName;
            }
            isMapped = false;
            return sourceFieldName;
        }

        private bool TryGetTargetFieldName(string workItemType, string sourceFieldName, out string targetFieldName)
        {
            if (SourceFieldMappings.TryGetValue(workItemType, out Dictionary<string, string> mappings))
            {
                if (mappings.TryGetValue(sourceFieldName, out targetFieldName))
                {
                    return true;
                }
            }
            targetFieldName = string.Empty;
            return false;
        }
    }
}
