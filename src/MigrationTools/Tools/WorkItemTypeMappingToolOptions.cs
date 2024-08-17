using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infra;

namespace MigrationTools.Tools
{
    public class WorkItemTypeMappingToolOptions : ToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:WorkItemTypeMappingTool";

        /// <summary>
        /// List of work item mappings. 
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; }

    }

    public class RegexWorkItemTypeMapping
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
        public string Description { get; set; }
    }
}