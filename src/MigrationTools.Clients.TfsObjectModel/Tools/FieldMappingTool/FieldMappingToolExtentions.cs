using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    internal static class FieldMappingToolExtentions
    {
        /// <summary>
        /// Returns all defined field maps of type <typeparamref name="TFieldMap"/> for work item type <paramref name="witName"/>.
        /// If no field map is defined for given work item type, empty collection is returned.
        /// </summary>
        /// <typeparam name="TFieldMap">Type of field maps to look for.</typeparam>
        /// <param name="fieldMappingTool">Field mapping tool.</param>
        /// <param name="witName">Work item type name.</param>
        public static IEnumerable<TFieldMap> GetFieldMaps<TFieldMap>(
            this IFieldMappingTool fieldMappingTool,
            string witName)
            where TFieldMap : IFieldMap
        {
            if (fieldMappingTool.Items.TryGetValue(witName, out List<IFieldMap>? fieldMaps))
            {
                return fieldMaps
                    .Where(fm => fm is TFieldMap)
                    .Cast<TFieldMap>();
            }
            return [];
        }

        /// <summary>
        /// Returns defined field maps of type <see cref="FieldValueMap"/> for work item type <paramref name="witName"/>,
        /// which are defined between fields <paramref name="sourceFieldReferenceName"/> and <paramref name="targetFieldReferenceName"/>.
        /// </summary>
        /// <param name="fieldMappingTool">Field mapping tool.</param>
        /// <param name="witName">Work item type name.</param>
        /// <param name="sourceFieldReferenceName">Source field reference name.</param>
        /// <param name="targetFieldReferenceName">Target field reference name.</param>
        public static IEnumerable<FieldValueMap> GetFieldValueMaps(
            this IFieldMappingTool fieldMappingTool,
            string witName,
            string sourceFieldReferenceName,
            string targetFieldReferenceName)
            => fieldMappingTool.GetFieldMaps<FieldValueMap>(witName)
                .Where(fvm => sourceFieldReferenceName.Equals(fvm.Config.sourceField, StringComparison.OrdinalIgnoreCase)
                    && targetFieldReferenceName.Equals(fvm.Config.targetField, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns all defined value mappings for work item type <paramref name="witName"/> which are defined between
        /// fields <paramref name="sourceFieldReferenceName"/> and <paramref name="targetFieldReferenceName"/>.
        /// </summary>
        /// <param name="fieldMappingTool">Field mapping tool.</param>
        /// <param name="witName">Work item type name.</param>
        /// <param name="sourceFieldReferenceName">Source field reference name.</param>
        /// <param name="targetFieldReferenceName">Target field reference name.</param>
        /// <returns>Dictionary with mappings source field values to target field values.</returns>
        /// <exception cref="InvalidOperationException">Thrown when there are defined more than one target values for
        /// specific source value.</exception>
        public static Dictionary<string, string> GetFieldValueMappings(
            this IFieldMappingTool fieldMappingTool,
            string witName,
            string sourceFieldReferenceName,
            string targetFieldReferenceName)
        {
            Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);

            IEnumerable<FieldValueMap> fieldValueMaps = fieldMappingTool
                .GetFieldValueMaps(witName, sourceFieldReferenceName, targetFieldReferenceName);
            foreach (FieldValueMap fieldValueMap in fieldValueMaps)
            {
                foreach (KeyValuePair<string, string> map in fieldValueMap.Config.valueMapping)
                {
                    string sourceValue = map.Key;
                    string targetValue = map.Value;
                    if (result.TryGetValue(sourceValue, out string existingTargetValue)
                        && !existingTargetValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase))
                    {
                        string msg = $"Conflict in field value mapping for '{sourceFieldReferenceName}' to '{targetFieldReferenceName}' in '{witName}': "
                            + $"Value '{sourceValue}' maps to both '{existingTargetValue}' and '{targetValue}'.";
                        throw new InvalidOperationException(msg);
                    }
                    result[sourceValue] = targetValue;
                }
            }
            return result;
        }
    }
}
