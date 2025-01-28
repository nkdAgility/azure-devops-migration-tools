using System;
using System.Collections.Generic;
using DotNet.Globbing;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsGitRepositoryToolOptions : ToolOptions
    {
        /// <summary>
        /// When set to True the changedset links will be dropped during the migration
        /// </summary>
        public bool ShouldDropChangedSetLinks { get; set; }

        /// <summary>
        /// List of work item mappings. 
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
    }

}