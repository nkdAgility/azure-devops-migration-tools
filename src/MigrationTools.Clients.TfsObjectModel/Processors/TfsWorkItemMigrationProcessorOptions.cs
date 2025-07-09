using System.Collections.Generic;
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
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool UpdateCreatedDate { get; set; } = true;

        /// <summary>
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool UpdateCreatedBy { get; set; } = true;

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
        /// <default>true</default>
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
        /// If enabled, adds a comment recording the migration
        /// </summary>
        /// <default>true</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool GenerateMigrationComment { get; set; } = true;

        /// <summary>
        /// A list of work items to import
        /// </summary>
        /// <default>[]</default>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<int> WorkItemIDs { get; set; }

        /// <summary>
        /// The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will
        /// continue until the number of failed items reaches the configured value, after which the migration fails.
        /// </summary>
        /// <default>0</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MaxGracefulFailures { get; set; } = 0;

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

        /// <summary>
        /// Validates if all fields in source work item exist in the target work item type.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ValidateWorkItemFields { get; set; } = false;

        /// <summary>
        /// Used when validating work item fields (<see cref="ValidateWorkItemFields"/> is <see langword="true"/>).
        /// If set to <see langword="true"/>, the migration will continue even if some fields in the source work item
        /// do not exist in the target work item type. If set to <see langword="false"/>, the migration will stop with
        /// error in this case. Default value is <see langword="false"/>.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ContinueIfMissingFieldsInTarget { get; set; } = false;
    }
}
