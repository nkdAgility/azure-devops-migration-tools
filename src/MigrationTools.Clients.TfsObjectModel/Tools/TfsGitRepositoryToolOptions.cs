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
    /// <summary>
    /// Configuration options for the TFS Git Repository Tool that handles Git repository mappings and changeset link transformations during work item migration.
    /// </summary>
    public class TfsGitRepositoryToolOptions : ToolOptions
    {
        /// <summary>
        /// When set to true, changeset links in work items will be removed during migration to prevent broken links when repositories are not migrated.
        /// </summary>
        public bool ShouldDropChangedSetLinks { get; set; }

        /// <summary>
        /// Dictionary mapping source repository names to target repository names. Used to update Git repository links and references in work items during migration. Keys may be a repository name ("Repo") or a project-qualified name ("Project/Repo"); the qualified form takes precedence. Repositories referenced from other (or renamed) projects are resolved by repository ID and rewritten when mapped here or when the target project contains a repository with the same name.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
    }

}