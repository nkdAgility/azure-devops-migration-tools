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
        /// Dictionary mapping source repository names to target repository names. Used to update Git repository links and references in work items during migration. Keys may be a repository name ("Repo") or a project-qualified name ("Project/Repo"); the qualified form takes precedence, so repositories with the same name in different source projects can be mapped separately. A bare "Repo" key that matches repositories in more than one source project logs a warning listing them. Values may also be "Project/Repo" to pick a repository in another target project. Repositories referenced from other (or renamed) projects are resolved by repository ID and rewritten when mapped here or when the target project contains a repository with the same name.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// When true, a commit or branch link is only rewritten to the target repository if that commit or branch exists there; otherwise the source link is kept and a warning names the work item, repository and commit. Set to false to rewrite every link without checking (the behaviour before this option existed). Pull request links are not affected; see DropPullRequestLinks.
        /// </summary>
        /// <default>true</default>
        public bool VerifyTargetExists { get; set; } = true;

        /// <summary>
        /// Pull requests are not carried by a git push, so a pull request link cannot be rewritten to the target. By default the link is kept pointing at the source pull request and a warning names the work item and pull request. Set to true to remove pull request links instead.
        /// </summary>
        /// <default>false</default>
        public bool DropPullRequestLinks { get; set; }
    }

}