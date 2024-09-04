using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Options
{
    public enum OptionsConfigurationTemplate
    {
        Reference = 0,
        WorkItemTracking = 1,
        Basic = 3,
        PipelineProcessor = 5
    }

    public static class OptionsConfigurationTemplateExtensions
    {

        public static OptionsConfigurationBuilder ApplyTemplate(this OptionsConfigurationBuilder optionsBuilder, OptionsConfigurationTemplate template)
        {
            switch (template)
            {
                case OptionsConfigurationTemplate.Reference:
                    optionsBuilder.AddAllOptions();
                    break;
                case OptionsConfigurationTemplate.Basic:
                case OptionsConfigurationTemplate.WorkItemTracking:
                    optionsBuilder.AddOption("TfsWorkItemMigrationProcessor");
                    optionsBuilder.AddOption("FieldMappingTool");
                    optionsBuilder.AddOption("FieldLiteralMap");
                    optionsBuilder.AddOption("TfsTeamProjectEndpoint", "Source");
                    optionsBuilder.AddOption("TfsTeamProjectEndpoint", "Target");
                    break;
                case OptionsConfigurationTemplate.PipelineProcessor:
                    optionsBuilder.AddOption("AzureDevOpsPipelineProcessor");
                    optionsBuilder.AddOption("AzureDevOpsEndpoint", "PipelineSource");
                    optionsBuilder.AddOption("AzureDevOpsEndpoint", "PipelineTarget");
                    break;
                default:
                    optionsBuilder.AddAllOptions();
                    break;
            }
            return optionsBuilder;
        }

    }
}
