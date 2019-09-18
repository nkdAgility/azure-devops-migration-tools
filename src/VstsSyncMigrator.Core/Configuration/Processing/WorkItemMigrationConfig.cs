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
        public bool Enabled { get; set; }
        /// <inheritdoc />
        public Type Processor => typeof(WorkItemMigrationContext);

        public bool LinkMigration { get; set; }
        public bool AttachmentMigration { get; set; }
        public string AttachmentWorkingPath { get; set; }

        public bool FixHtmlAttachmentLinks { get; set; }


        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}