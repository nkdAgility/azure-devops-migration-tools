using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemMigrationConfig : ITfsProcessingConfig
    {
        public bool ReplayRevisions { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
        public bool UpdateSourceReflectedId { get; set; }
        public bool BuildFieldTable { get; set; }
        public bool AppendMigrationToolSignatureFooter { get; set; }
        public string QueryBit { get; set; }
        /// <inheritdoc />
        public string OrderBit { get; set; }
        public bool Enabled { get; set; }
        /// <inheritdoc />
        public Type Processor => typeof(WorkItemMigrationContext);

        public bool LinkMigration { get; set; }
        public bool AttachmentMigration { get; set; }
        public string AttachmentWorkingPath { get; set; }

        public bool FixHtmlAttachmentLinks { get; set; }

        public bool SkipToFinalRevisedWorkItemType { get; set; }

        public int WorkItemCreateRetryLimit { get; set; }

        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int AttachmentMazSize { get; set; }
        public bool CollapseRevisions { get; set; }
        public bool LinkMigrationSaveEachAsAdded { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
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
            AttachmentMazSize = 480000000;
            UpdateCreatedBy = true;
            PrefixProjectToNodes = false;
            UpdateCreatedDate = true;
            LinkMigrationSaveEachAsAdded = false;
            UpdateSourceReflectedId = false;
            QueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            OrderBit = "[System.ChangedDate] desc";
        }
    }
}