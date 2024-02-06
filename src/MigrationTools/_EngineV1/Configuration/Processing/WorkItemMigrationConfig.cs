using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{

    public class WorkItemMigrationConfig : IWorkItemProcessorConfig
    {
        /// <summary>
        /// You can choose to migrate the tip only (a single write) or all of the revisions (many writes).
        /// If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true`.
        /// </summary>
        /// <default>true</default>
        public bool ReplayRevisions { get; set; }

        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        public bool UpdateCreatedDate { get; set; }

        /// <summary>
        /// If this is enabled the creation process on the target project will create the items with the original creation date.
        /// (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
        /// not the internal create date)
        /// </summary>
        /// <default>true</default>
        public bool UpdateCreatedBy { get; set; }

        /// <summary>
        /// A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
        /// </summary>
        /// <default>AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')</default>
        public string WIQLQueryBit { get; set; }

        /// <summary>
        /// A work item query to affect the order in which the work items are migrated. Don't leave this empty.
        /// </summary>
        /// <default>[System.ChangedDate] desc</default>
        public string WIQLOrderBit { get; set; }

        /// <summary>
        /// If enabled then the processor will run
        /// </summary>
        /// <default>false</default>
        public bool Enabled { get; set; }

        /// <summary>
        /// Name used to identify this processor
        /// </summary>
        /// <default>?</default>
        public string Processor => "WorkItemMigrationContext";

        /// <summary>
        /// If enabled this will migrate the Links for the work item at the same time as the whole work item.
        /// </summary>
        /// <default>true</default>
        public bool LinkMigration { get; set; }

        /// <summary>
        /// If enabled this will migrate all of the attachments at the same time as the work item
        /// </summary>
        /// <default>true</default>
        public bool AttachmentMigration { get; set; }

        /// <summary>
        /// `AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally.
        /// </summary>
        /// <default>C:\temp\Migration\</default>
        public string AttachmentWorkingPath { get; set; }

        /// <summary>
        /// **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML
        /// fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps;
        /// TFS should use integrated authentication.
        /// </summary>
        /// <default>?</default>
        public bool FixHtmlAttachmentLinks { get; set; }

        /// <summary>
        /// **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML fields as well as discussion comments. You must specify a
        /// PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication.
        /// </summary>
        /// <default>false</default>
        public bool SkipToFinalRevisedWorkItemType { get; set; }

        /// <summary>
        /// **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count.
        /// This allows for periodic network glitches not to end the process.
        /// </summary>
        /// <default>5</default>
        public int WorkItemCreateRetryLimit { get; set; }

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
        public bool PauseAfterEachWorkItem { get; set; }

        /// <summary>
        /// `AttachmentMigration` is set to true then you need to specify a max file size for upload in bites.
        /// For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb).
        /// </summary>
        /// <default>480000000</default>
        public int AttachmentMaxSize { get; set; }

        /// <summary>
        /// This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`.
        /// </summary>
        /// <default>?</default>
        public bool AttachRevisionHistory { get; set; }

        /// <summary>
        /// If you have changed parents before re-running a sync you may get a `TF26194: unable to change the value of the 'Parent' field` error.
        /// This will resolve it, but will slow migration.
        /// </summary>
        /// <default>false</default>
        public bool LinkMigrationSaveEachAsAdded { get; set; }

        /// <summary>
        /// If enabled, adds a comment recording the migration
        /// </summary>
        /// <default>false</default>
        public bool GenerateMigrationComment { get; set; }

        /// <summary>
        /// A list of work items to import
        /// </summary>
        /// <default>[]</default>
        public IList<int> WorkItemIDs { get; set; }

        /// <summary>
        /// Sets the maximum number of revisions that will be migrated. "First + Last N = Max".
        /// If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated.
        /// </summary>
        /// <default>0</default>
        public int MaxRevisions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <default>?</default>
        public bool UseCommonNodeStructureEnricherConfig { get; set; }

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
        /// The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will
        /// continue until the number of failed items reaches the configured value, after which the migration fails.
        /// </summary>
        /// <default>0</default>
        public int MaxGracefulFailures { get; set; }

        /// <summary>
        /// This will skip a revision if the source iteration has not been migrated i.e. it was deleted
        /// </summary>
        public bool SkipRevisionWithInvalidIterationPath { get; set; }

        /// <summary>
        /// When set to true, this setting will skip a revision if the source area has not been migrated, has been deleted or is somehow invalid, etc.
        /// </summary>
        public bool SkipRevisionWithInvalidAreaPath { get; set; }

        /// <summary>
        /// When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
        /// </summary>
       public bool ShouldCreateMissingRevisionPaths { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        /// <summary>
        /// Creates a new workitemmigrationconfig with default values
        /// </summary>
        public WorkItemMigrationConfig()
        {
            Enabled = false;
            WorkItemCreateRetryLimit = 5;
            FilterWorkItemsThatAlreadyExistInTarget = false;
            ReplayRevisions = true;
            LinkMigration = true;
            AttachmentMigration = true;
            FixHtmlAttachmentLinks = false;
            AttachmentWorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
            AttachmentMaxSize = 480000000;
            UpdateCreatedBy = true;
            PrefixProjectToNodes = false;
            UpdateCreatedDate = true;
            SkipToFinalRevisedWorkItemType = false;
            LinkMigrationSaveEachAsAdded = false;
            GenerateMigrationComment = true;
            WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')";
            WIQLOrderBit = "[System.ChangedDate] desc";
            MaxRevisions = 0;
            AttachRevisionHistory = false;
            AreaMaps = new Dictionary<string, string>();
            IterationMaps = new Dictionary<string, string>();
            MaxGracefulFailures = 0;
            SkipRevisionWithInvalidIterationPath = false;
            SkipRevisionWithInvalidAreaPath = false;
            ShouldCreateMissingRevisionPaths = true;
        }
    }
}