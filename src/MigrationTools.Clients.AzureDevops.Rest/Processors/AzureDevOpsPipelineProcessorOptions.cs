using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class AzureDevOpsPipelineProcessorOptions : ProcessorOptions
    {
        public AzureDevOpsPipelineProcessorOptions()
        {
            Enabled = false;
            MigrateBuildPipelines = true;
            MigrateReleasePipelines = true;
            MigrateTaskGroups = true;
            MigrateVariableGroups = true;
            MigrateServiceConnections = true;
            BuildPipelines = null;
            ReleasePipelines = null;
            SourceName = "sourceName";
            TargetName = "targetName";
        }
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
        /// Migrate Service Connections **secrets need to be entered manually**
        /// </summary>
        /// <default>true</default>
        public bool MigrateServiceConnections { get; set; }

        /// <summary>
        /// List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed.
        /// </summary>
        public List<string> BuildPipelines { get; set; }

        /// <summary>
        /// List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed.
        /// </summary>
        public List<string> ReleasePipelines { get; set; }

        /// <summary>
        /// Map of Source Repository to Target Repository Names 
        /// </summary>
        public Dictionary<string, string> RepositoryNameMaps { get; set; } //Can we reuse GitRepoMapping?

    }
}