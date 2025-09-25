using System;

namespace MigrationTools.Tools
{
    internal enum FieldMappingType
    {
        None,
        Skip,
        SourceToTarget,
        TargetToTarget
    }

    internal class FieldMappingInfo
    {
        public static readonly FieldMappingInfo None = new(FieldMappingType.None, string.Empty, string.Empty);
        public static readonly FieldMappingInfo Skip = new(FieldMappingType.Skip, string.Empty, string.Empty);

        public FieldMappingInfo CreateSourceToTarget(string sourceFieldName, string targetFieldName)
        {
            if (string.IsNullOrWhiteSpace(sourceFieldName))
            {
                const string msg = $"Source field name cannot be empty for '{nameof(FieldMappingType.SourceToTarget)}' mappping type.";
                throw new ArgumentException(nameof(sourceFieldName), msg);
            }
            return new(FieldMappingType.SourceToTarget, sourceFieldName, targetFieldName);
        }

        public FieldMappingInfo CreateTargetToTarget(string sourceFieldName, string targetFieldName)
        {
            if (string.IsNullOrWhiteSpace(sourceFieldName))
            {
                const string msg = $"Source field name cannot be empty for '{nameof(FieldMappingType.TargetToTarget)}' mappping type.";
                throw new ArgumentException(nameof(sourceFieldName), msg);
            }
            return new(FieldMappingType.TargetToTarget, sourceFieldName, targetFieldName);
        }

        private FieldMappingInfo(FieldMappingType type, string sourceFieldName, string targetFieldName)
        {
            MappingType = type;
            SourceFieldName = sourceFieldName;
            TargetFieldName = targetFieldName;
        }

        public FieldMappingType MappingType { get; }
        public string SourceFieldName { get; }
        public string TargetFieldName { get; }
    }
}
