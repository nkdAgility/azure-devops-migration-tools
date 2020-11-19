using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    public class TfsTeamSettingsProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Migrate original team settings after their creation on target team project
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamSettings { get; set; }

        /// <summary>
        /// Reset the target team settings to match the source if the team exists
        /// </summary>
        /// <default>false</default>
        public bool UpdateTeamSettings { get; set; }

        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// List of Teams to process. If this is `null` then all teams will be processed.
        /// </summary>
        public List<string> Teams { get; set; }

        public override Type ToConfigure => typeof(TfsTeamSettingsProcessor);

        public override string Processor => ToConfigure.Name;

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            MigrateTeamSettings = true;
            UpdateTeamSettings = true;
            PrefixProjectToNodes = false;
            var e1 = new TfsTeamSettingsEndpointOptions();
            e1.SetDefaults();
            e1.Project = "sourceProject";
            Source = e1;
            var e2 = new TfsTeamSettingsEndpointOptions();
            e2.SetDefaults();
            e2.Project = "targetProject";
            Target = e2;
        }
    }
}