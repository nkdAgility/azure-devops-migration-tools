using System;
using System.Collections.Generic;
using MigrationTools.Options;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class WorkItemTypeMappingEnricherOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:WorkItemTypeMappingEnricher";

        public override Type ToConfigure => typeof(WorkItemTypeMappingEnricher);


        /// <summary>
        /// List of work item mappings. 
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            WorkItemTypeDefinition = new Dictionary<string, string> { { "Default", "Default2" } };
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