using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Container for common tools used across migration operations, providing centralized access to frequently used utility tools.
    /// </summary>
    public class CommonTools
    {
        /// <summary>
        /// Gets the string manipulator tool for processing string fields.
        /// </summary>
        public IStringManipulatorTool StringManipulator { get; private set; }

        /// <summary>
        /// Gets the work item type mapping tool for transforming work item types.
        /// </summary>
        public IWorkItemTypeMappingTool WorkItemTypeMapping { get; private set; }

        /// <summary>
        /// Gets the work item fields mapping tool for checking work item types compatibility.
        /// </summary>
        public IFieldReferenceNameMappingTool FieldReferenceNameMappingTool { get; private set; }

        /// <summary>
        /// Gets the field mapping tool for applying field transformations.
        /// </summary>
        public IFieldMappingTool FieldMappingTool { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CommonTools class.
        /// </summary>
        /// <param name="StringManipulatorTool">Tool for string field manipulation.</param>
        /// <param name="workItemTypeMapping">Tool for work item type mapping.</param>
        /// <param name="fieldReferenceNameMappingTool">Tool for work item fields mapping.</param>
        /// <param name="fieldMappingTool">Tool for field mapping operations.</param>
        public CommonTools(
            IStringManipulatorTool StringManipulatorTool,
            IWorkItemTypeMappingTool workItemTypeMapping,
            IFieldReferenceNameMappingTool fieldReferenceNameMappingTool,
            IFieldMappingTool fieldMappingTool)
        {
            StringManipulator = StringManipulatorTool;
            WorkItemTypeMapping = workItemTypeMapping;
            FieldReferenceNameMappingTool = fieldReferenceNameMappingTool;
            FieldMappingTool = fieldMappingTool;
        }

    }
}
