using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TfsWorkItemMigrationProcessor, which handles comprehensive work item migration including revisions, links, and attachments.
    /// </summary>
    public class TfsWorkItemMigrationProcessorOptions : ProcessorOptions, IWorkItemProcessorConfig
    {
        /// <summary>
        /// A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
        /// </summary>
        /// <default>SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc</default>
        [Required]
        public string WIQLQuery { get; set; }

        /// <summary>
        /// **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML
        /// fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps;
        /// TFS should use integrated authentication.
        /// </summary>
        /// <default>true</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool FixHtmlAttachmentLinks { get; set; } = true;

        /// <summary>
        /// **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count.
        /// This allows for periodic network glitches not to end the process.
        /// </summary>
        /// <default>5</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int WorkItemCreateRetryLimit { get; set; } = 5;

        /// <summary>
        /// This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run.
        /// While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
        /// </summary>
        /// <default>false</default>
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }

        /// <summary>
        /// Pause after each work item is migrated
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool PauseAfterEachWorkItem { get; set; } = false;

        /// <summary>
        /// This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`.
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AttachRevisionHistory { get; set; } = false;

        /// <summary>
        /// If enabled, adds a comment recording the migration. The comment is added to the History of the last replayed
        /// revision, after that revision's own comment, rather than in a separate revision.
        /// </summary>
        /// <default>true</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool GenerateMigrationComment { get; set; } = true;

        /// <summary>
        /// If enabled, adds a hyperlink with the comment `Migrated from source` from each migrated work item to the web page of
        /// its source work item, in addition to the reflected work item ID and the migration comment. The link is added in the
        /// last replayed revision, or in a save that a re-run makes anyway, so it never adds a revision. Re-runs keep exactly one
        /// such link: if the source URL has changed, the old migration hyperlink is replaced. A missing link is added the next time
        /// a migration run processes and saves the item, not by a save made in Azure DevOps. If the target already has a hyperlink
        /// to the source URL with a different comment, it counts as present and is neither duplicated nor changed; this is how it
        /// was built and Martin has not approved it yet. The exception is a `Migrated from source`
        /// hyperlink that the source work item has itself (for example because the source was itself migrated): it is copied and
        /// kept, so the target can then have two.
        /// </summary>
        /// <default>true</default>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool GenerateMigrationHyperlink { get; set; } = true;

        /// <summary>
        /// A list of work items to import
        /// </summary>
        /// <default>[]</default>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<int> WorkItemIDs { get; set; }

        /// <summary>
        /// The maximum number of failed work items to tolerate before the run is aborted. When set to zero (the default) there is no limit: every failed
        /// work item is logged and recorded, the remaining items are still migrated, and the processor is reported as failed at the end with a list of the
        /// failed IDs. When set above zero, the run aborts once more than this number of items have failed. Use `StopOnFirstError` to stop on the first failure.
        /// </summary>
        /// <default>0</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MaxGracefulFailures { get; set; } = 0;

        /// <summary>
        /// When true, the run stops immediately on the first work item that fails to migrate (the previous default behaviour). When false (the default),
        /// the failed item is recorded and the remaining items are migrated; the failed IDs are summarised at the end and the process exits non-zero.
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool StopOnFirstError { get; set; } = false;

        /// <summary>
        /// This will skip a revision if the source iteration has not been migrated i.e. it was deleted
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool SkipRevisionWithInvalidIterationPath { get; set; } = false;

        /// <summary>
        /// When set to true, this setting will skip a revision if the source area has not been migrated, has been deleted or is somehow invalid, etc.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool SkipRevisionWithInvalidAreaPath { get; set; } = false;
    }
}
