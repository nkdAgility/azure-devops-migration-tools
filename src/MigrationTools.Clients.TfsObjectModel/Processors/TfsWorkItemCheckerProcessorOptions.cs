using System;
using System.Collections.Generic;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class TfsWorkItemCheckerProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Special key, meaning this mapping applied to all work item types.
        /// </summary>
        public const string AllWorkItemTypes = "*";

        private static readonly StringComparer _normalizedComparer = StringComparer.OrdinalIgnoreCase;
        private bool _isNormalized = false;

        public Dictionary<string, Dictionary<string, string>> TargetFieldsMappings { get; set; } = [];

        /// <summary>
        /// If set to <see langword="true"/>, migration processor will stop with error if some fields or work item types are
        /// missing in target. If set to <see langword="false"/>, it will log all missing fields and the migration will continue.
        /// Default value is <see langword="true"/>.
        /// </summary>
        public bool StopIfMissingFieldsInTarget { get; set; } = true;

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
            if (TargetFieldsMappings.TryGetValue(workItemType, out Dictionary<string, string> mappings))
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
        /// Normalizes <see cref="TargetFieldsMappings"/> so all the dictionaries uses case-insensitive keys.
        /// </summary>
        public void Normalize()
        {
            if (_isNormalized)
            {
                return;
            }

            Dictionary<string, Dictionary<string, string>> oldMappings = TargetFieldsMappings;
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

            _isNormalized = true;
            TargetFieldsMappings = newMappings;
        }
    }
}
