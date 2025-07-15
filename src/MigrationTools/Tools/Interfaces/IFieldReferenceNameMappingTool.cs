namespace MigrationTools.Tools.Interfaces
{
    /// <summary>
    /// Tool for mapping fields from source to target. This tool is for just checking if all fields in source really
    /// exists in target. It does not perform any copying or transformation of field values´between source and target.
    /// To work with values, use <see cref="IFieldMappingTool"/>.
    /// </summary>
    public interface IFieldReferenceNameMappingTool
    {
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
        string GetTargetFieldName(string workItemType, string sourceFieldName, out bool isMapped);
    }
}
