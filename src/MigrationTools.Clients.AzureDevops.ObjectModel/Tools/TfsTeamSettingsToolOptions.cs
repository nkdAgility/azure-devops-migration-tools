using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsTeamSettingsToolOptions : ProcessorEnricherOptions, ITfsTeamSettingsToolOptions
    {

        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsTeamSettingsTool";
        public override Type ToConfigure => typeof(TfsTeamSettingsTool);

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
        /// Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored.
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamCapacities { get; set; }

        /// <summary>
        /// List of Teams to process. If this is `null` then all teams will be processed.
        /// </summary>
        public List<string> Teams { get; set; }

        public override void SetDefaults()
        {
            Enabled = false;
            MigrateTeamSettings = true;
            UpdateTeamSettings = true;
            MigrateTeamCapacities = true;
        }

        static public TfsTeamSettingsToolOptions GetDefaults()
        {
            var result = new TfsTeamSettingsToolOptions();
            result.SetDefaults();
            return result;
        }
    }

    public interface ITfsTeamSettingsToolOptions
    {

        public bool MigrateTeamSettings { get; set; }

        public bool UpdateTeamSettings { get; set; }

        public bool MigrateTeamCapacities { get; set; }

        public List<string> Teams { get; set; }
    }
}