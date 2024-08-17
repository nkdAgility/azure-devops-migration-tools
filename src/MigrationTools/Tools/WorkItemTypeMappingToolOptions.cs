using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Tools
{
    public class WorkItemTypeMappingToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:WorkItemTypeMappingTool";

        public override Type ToConfigure => typeof(WorkItemTypeMappingTool);


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

    public class RegexWorkItemTypeMapping
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
        public string Description { get; set; }
    }
}