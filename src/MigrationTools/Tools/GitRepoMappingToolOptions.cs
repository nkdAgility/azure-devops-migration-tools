using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Tools
{
    public class GitRepoMappingToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:GitRepoMappingTool";

        public override Type ToConfigure => typeof(GitRepoMappingTool);


        /// <summary>
        /// List of work item mappings. 
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            Mappings = new Dictionary<string, string> { { "Default", "Default2" } };
        }
    }

}