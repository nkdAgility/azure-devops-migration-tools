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
        /// The root paths of the Ares / Iterations you want migrate. See [NodeBasePath Configuration](#nodebasepath-configuration)
        /// </summary>
        /// <default>["/"]</default>
        public string[] NodeBasePaths { get; set; }

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
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }
    }
}