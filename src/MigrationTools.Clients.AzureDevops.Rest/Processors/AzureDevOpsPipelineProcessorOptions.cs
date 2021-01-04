using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    public class AzureDevOpsPipelineProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Migrate Build Pipelines
        /// </summary>
        /// <default>true</default>
        public bool MigrateBuildPipelines { get; set; }

        /// <summary>
        /// Migrate Release Pipelines
        /// </summary>
        /// <default>true</default>
        public bool MigrateReleasePipelines { get; set; }

        /// <summary>
        /// Migrate Task Groups
        /// </summary>
        /// <default>true</default>
        public bool MigrateTaskGroups { get; set; }

        /// <summary>
        /// Migrate Valiable Groups
        /// </summary>
        /// <default>true</default>
        public bool MigrateVariableGroups { get; set; }

        /// <summary>
        /// Migrate Service Connections
        /// </summary>
        /// <default>true</default>
        public bool MigrateServiceConnections { get; set; }

        /// <summary>
        /// List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed. **Not implemented yet**
        /// </summary>
        public List<string> BuildPipelines { get; set; }

        /// <summary>
        /// List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed. **Not implemented yet**
        /// </summary>
        public List<string> ReleasePipelines { get; set; }

        public override Type ToConfigure => typeof(AzureDevOpsPipelineProcessor);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            MigrateBuildPipelines = true;
            MigrateReleasePipelines = true;
            MigrateTaskGroups = true;
            MigrateVariableGroups = true;
            MigrateServiceConnections = true;
            BuildPipelines = null;
            ReleasePipelines = null;
            var e1 = new AzureDevOpsEndpointOptions();
            e1.SetDefaults();
            e1.Project = "sourceProject";
            Source = e1;
            var e2 = new AzureDevOpsEndpointOptions();
            e2.SetDefaults();
            e2.Project = "targetProject";
            Target = e2;
        }
    }
}