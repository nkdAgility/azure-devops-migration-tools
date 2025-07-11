using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Tool for mapping fields from source to target. This tool is for just checking if all fields in source really
    /// exists in target. It does not perform any copying or transformation of field values´between source and target.
    /// To work with values, use <see cref="FieldMappingTool"/>.
    /// </summary>
    public class FieldReferenceNameMappingTool : Tool<FieldReferenceNameMappingToolOptions>, IFieldReferenceNameMappingTool
    {
        public FieldReferenceNameMappingTool(
            IOptions<FieldReferenceNameMappingToolOptions> options,
            IServiceProvider services,
            ILogger<ITool>
            logger,
            ITelemetryLogger telemetry)
            : base(new OptionsWrapper<FieldReferenceNameMappingToolOptions>(options.Value.Normalize()),
                  services, logger, telemetry)
        {
        }

        /// <inheritdoc/>
        public string GetTargetFieldName(string workItemType, string sourceFieldName, out bool isMapped)
            => Options.GetTargetFieldName(workItemType, sourceFieldName, out isMapped);
    }
}
