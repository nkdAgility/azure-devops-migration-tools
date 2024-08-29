﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsValidateRequiredFieldTool : Tool<TfsValidateRequiredFieldToolOptions>
    {

        public TfsValidateRequiredFieldTool(IOptions<TfsValidateRequiredFieldToolOptions> options, IServiceProvider services, ILogger<TfsValidateRequiredFieldTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
        }


        public IMigrationEngine Engine { get; private set; }

        public bool ValidatingRequiredField(TfsProcessor processor, string fieldToFind, List<WorkItemData> sourceWorkItems)
        {
            var workItemTypeMappingTool = Services.GetRequiredService<WorkItemTypeMappingTool>();
            var sourceWorkItemTypes = sourceWorkItems.Select(wid => wid.ToWorkItem().Type).Distinct();
            var targetTypes = processor.Target.WorkItems.Project.ToProject().WorkItemTypes;
            var result = true;
            foreach (WorkItemType sourceWorkItemType in sourceWorkItemTypes)
            {
                try
                {

                    var workItemTypeName = sourceWorkItemType.Name;
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
                    Log.LogWarning(ex, "ValidatingRequiredField: Unable to validate one of the work items as its returned by TFS but has been deleted");
                }

            }
            return result;
        }

        //private void ConfigValidation()
        //{
        //    //Make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type
        //    var fields = _witClient.GetFieldsAsync(Engine.Target.Config.AsTeamProjectConfig().Project).Result;
        //    bool rwiidFieldExists = fields.Any(x => x.ReferenceName == Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName || x.Name == Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
        //    contextLog.Information("Found {FieldsFoundCount} work item fields.", fields.Count.ToString("n0"));
        //    if (rwiidFieldExists)
        //        contextLog.Information("Found '{ReflectedWorkItemIDFieldName}' in this project, proceeding.", Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
        //    else
        //    {
        //        contextLog.Information("Config file specifies '{ReflectedWorkItemIDFieldName}', which wasn't found.", Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
        //        contextLog.Information("Instead, found:");
        //        foreach (var field in fields.OrderBy(x => x.Name))
        //            contextLog.Information("{FieldType} - {FieldName} - {FieldRefName}", field.Type.ToString().PadLeft(15), field.Name.PadRight(20), field.ReferenceName ?? "");
        //        throw new Exception("Running a replay migration requires a ReflectedWorkItemId field to be defined in the target project's process.");
        //    }
        //}
    }
}