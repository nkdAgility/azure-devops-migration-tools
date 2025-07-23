using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Tool for validating that required fields exist in target work item types before migration, preventing migration failures due to missing required fields.
    /// </summary>
    public class TfsValidateRequiredFieldTool : Tool<TfsValidateRequiredFieldToolOptions>
    {
        /// <summary>
        /// Initializes a new instance of the TfsValidateRequiredFieldTool class.
        /// </summary>
        /// <param name="options">Configuration options for the validation tool</param>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for the tool operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public TfsValidateRequiredFieldTool(IOptions<TfsValidateRequiredFieldToolOptions> options, IServiceProvider services, ILogger<TfsValidateRequiredFieldTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
        }


        /// <summary>
        /// Gets the migration engine instance for accessing migration context.
        /// </summary>
        public IMigrationEngine Engine { get; private set; }

        /// <summary>
        /// Validates that a required field exists in all target work item types that correspond to the source work items.
        /// </summary>
        /// <param name="processor">The TFS processor performing the migration</param>
        /// <param name="fieldToFind">The name of the field to validate</param>
        /// <param name="sourceWorkItems">The source work items to validate against</param>
        /// <returns>True if all target work item types have the required field; otherwise, false</returns>
        public bool ValidatingRequiredField(TfsProcessor processor, string fieldToFind, List<WorkItemData> sourceWorkItems)
        {
            var workItemTypeMappingTool = Services.GetRequiredService<IWorkItemTypeMappingTool>();
            var sourceWorkItemTypes = sourceWorkItems.Select(wid => wid.ToWorkItem().Type).Distinct();
            var targetTypes = processor.Target.WorkItems.Project.ToProject().WorkItemTypes;
            var result = true;
            foreach (WorkItemType sourceWorkItemType in sourceWorkItemTypes)
            {
                try
                {

                    var workItemTypeName = sourceWorkItemType.Name;
                    if (Options.Exclusions.Any(exclusion => string.Equals(exclusion, sourceWorkItemType.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        Log.LogDebug("ValidatingRequiredField: Skipping excluded work item type {WorkItemTypeName}", workItemTypeName);
                        continue;
                    }
                    if (workItemTypeMappingTool.Mappings != null && workItemTypeMappingTool.Mappings.ContainsKey(workItemTypeName))
                    {
                        workItemTypeName = workItemTypeMappingTool.Mappings[workItemTypeName];
                    }
                    var targetType = targetTypes[workItemTypeName];

                    if (targetType.FieldDefinitions.Contains(fieldToFind))
                    {
                        Log.LogDebug("ValidatingRequiredField: {WorkItemTypeName} contains {fieldToFind}", targetType.Name, fieldToFind);
                    }
                    else
                    {
                        Log.LogWarning("ValidatingRequiredField: {WorkItemTypeName} does not contain {fieldToFind}", targetType.Name, fieldToFind);
                        result = false;
                    }
                }
                catch (WorkItemTypeDeniedOrNotExistException ex)
                {
                    Log.LogWarning(ex, "ValidatingRequiredField: Unable to validate work item type {name} as its returned by the source but does not exist in the target", sourceWorkItemType.Name);
                }

            }
            return result;
        }

    }
}
