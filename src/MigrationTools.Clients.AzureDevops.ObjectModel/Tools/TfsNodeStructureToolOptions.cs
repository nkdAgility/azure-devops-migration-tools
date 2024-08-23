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
        /// Using the Glob format you can specify a list of nodes that you want to match. This can be used to filter the main migration of current nodes. note: This does not negate the nees for all nodes in the history of a work item in scope for the migration MUST exist for the system to run, and this will be validated before the migration. 
        /// </summary>
        /// <default>["/"]</default>
        public string[] Filters { get; set; }

        /// <summary>
        /// Remapping rules for area paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`,
        /// that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> AreaMaps { get; set; }

        /// <summary>
        /// Remapping rules for iteration paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`,
        /// that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> IterationMaps { get; set; }

        /// <summary>
        /// When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
        /// </summary>
        public bool ShouldCreateMissingRevisionPaths { get; set; }
        public bool ReplicateAllExistingNodes { get; set; }
    }

    public interface ITfsNodeStructureToolOptions
    {
        public string[] Filters { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }
    }
}