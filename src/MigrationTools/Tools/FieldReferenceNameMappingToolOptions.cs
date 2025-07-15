using System;
using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// <para>
    /// Configuration options for the <see cref="FieldReferenceNameMappingTool"/> defining how work item fields
    /// are mapped between source and target systems. Mapped target value can be empty string to indicate
    /// that the source field is not mapped to target at all.
    /// </para>
    /// <para>
    /// Field mappings can be defined for specific work item type, or for all work item types by using special work item
    /// type name <c>*</c> (<see cref="AllWorkItemTypes"/>). If no mapping is defined for the source field, the same value
    /// is returned as target field. For correct handling all this, use <see cref="GetTargetFieldName(string, string)"/> method.
    /// </para>
    /// <para>
    /// Before use, this options should be normalized using <see cref="Normalize"/> method to ensure that all comparisons
    /// of work item and field names are case-insensitive.
    /// </para>
    /// </summary>
    public class FieldReferenceNameMappingToolOptions : ToolOptions
    {
        /// <summary>
        /// Special key, meaning this mapping applied to all work item types.
        /// </summary>
        public const string AllWorkItemTypes = "*";

        private static readonly StringComparer _normalizedComparer = StringComparer.OrdinalIgnoreCase;
        private bool _isNormalized = false;

        /// <summary>
        /// Field reference name mappings. Key is work item type name, value is dictionary of mapping source filed name to
        /// target field name. Target field name can be empty string to indicate that the source field is not mapped
        /// to target at all. As work item type name, you can use <see cref="AllWorkItemTypes"/> to define mappings which will
        /// be applied to all work item types.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Mappings { get; set; } = [];

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
            if (Enabled)
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
            }
            isMapped = false;
            return sourceFieldName;
        }

        private bool TryGetTargetFieldName(string workItemType, string sourceFieldName, out string targetFieldName)
        {
            if (Mappings.TryGetValue(workItemType, out Dictionary<string, string> mappings))
            {
                if (mappings.TryGetValue(sourceFieldName, out targetFieldName))
                {
                    return true;
                }
            }
            targetFieldName = string.Empty;
            return false;
        }

        /// <summary>
        /// Creates a new instance with the same values as current, just the dictionaries lookups are case-insensitive
        /// and allk eys and values are trimmed.
        /// </summary>
        public FieldReferenceNameMappingToolOptions Normalize()
        {
            if (_isNormalized)
            {
                return this;
            }

            FieldReferenceNameMappingToolOptions result = new()
            {
                _isNormalized = true,
                Enabled = Enabled,
                Mappings = new(_normalizedComparer)
            };

            if (Mappings is not null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> mapping in Mappings)
                {
                    Dictionary<string, string> normalizedValues = new(_normalizedComparer);
                    foreach (KeyValuePair<string, string> fieldMapping in mapping.Value)
                    {
                        normalizedValues[fieldMapping.Key.Trim()] = fieldMapping.Value.Trim();
                    }
                    result.Mappings[mapping.Key.Trim()] = normalizedValues;
                }
            }

            return result;
        }
    }
}
