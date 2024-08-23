using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json.Schema;

namespace MigrationTools.Tools
{

    public sealed class TfsNodeStructureToolOptions : ToolOptions, ITfsNodeStructureToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsNodeStructureTool";

        /// <summary>
        /// Rules to apply to the Area Path. Is an object of NodeOptions e.g. { "Filters": ["*/**"], "Mappings": { "^oldProjectName([\\\\]?.*)$": "targetProjectA$1", } }
        /// </summary>
        /// <default>{"Filters": [], "Mappings": { "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1" })</default>
        public NodeOptions Areas { get; set; }

        /// <summary>
        /// Rules to apply to the Area Path. Is an object of NodeOptions e.g. { "Filters": ["*/**"], "Mappings": { "^oldProjectName([\\\\]?.*)$": "targetProjectA$1", } }
        /// </summary>
        /// <default>{"Filters": [], "Mappings": { "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1" })</default>
        public NodeOptions Iterations { get; set; }

        /// <summary>
        /// When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
        /// </summary>
        public bool ShouldCreateMissingRevisionPaths { get; set; }
        public bool ReplicateAllExistingNodes { get; set; }
    }

    public class NodeOptions
    {
        /// <summary>
        /// Using the Glob format you can specify a list of nodes that you want to match. This can be used to filter the main migration of current nodes. note: This does not negate the nees for all nodes in the history of a work item in scope for the migration MUST exist for the system to run, and this will be validated before the migration. e.g. add "migrationSource1\\Team 1,migrationSource1\\Team 1\\**" to match both the Team 1 node and all child nodes. 
        /// </summary>
        /// <default>["/"]</default>
        public List<string> Filters { get; set; }
        /// <summary>
        /// Remapping rules for nodes, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`,
        /// that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; }
    }

    public interface ITfsNodeStructureToolOptions
    {
        public NodeOptions Areas { get; set; }
        public NodeOptions Iterations { get; set; }
    }
}