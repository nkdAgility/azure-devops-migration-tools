using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class WorkItemMigrationConfig : IWorkItemProcessorConfig
    {
        public bool ReplayRevisions { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }

        public string WIQLQueryBit { get; set; }

        /// <inheritdoc />
        public string WIQLOrderBit { get; set; }

        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor => "WorkItemMigrationContext";

        public bool LinkMigration { get; set; }
        public bool AttachmentMigration { get; set; }
        public string AttachmentWorkingPath { get; set; }

        public bool FixHtmlAttachmentLinks { get; set; }

        public bool SkipToFinalRevisedWorkItemType { get; set; }

        public int WorkItemCreateRetryLimit { get; set; }

        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int AttachmentMaxSize { get; set; }
        public bool AttachRevisionHistory { get; set; }
        public bool LinkMigrationSaveEachAsAdded { get; set; }
        public bool GenerateMigrationComment { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        public int MaxRevisions { get; set; }

        public bool? NodeStructureEnricherEnabled { get; set; }
        public bool UseCommonNodeStructureEnricherConfig { get; set; }
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }


        public int MaxGracefulFailures { get; set; }

        /// <summary>
        /// This will skip a revision if the source iteration has not been migrated i.e. it was deleted
        /// </summary>
        public bool SkipRevisionWithInvalidIterationPath { get; set; }

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
            FilterWorkItemsThatAlreadyExistInTarget = true;
            ReplayRevisions = true;
            LinkMigration = true;
            AttachmentMigration = true;
            FixHtmlAttachmentLinks = false;
            AttachmentWorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
            AttachmentMaxSize = 480000000;
            UpdateCreatedBy = true;
            PrefixProjectToNodes = false;
            UpdateCreatedDate = true;
            SkipToFinalRevisedWorkItemType = true;
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
        }
    }
}