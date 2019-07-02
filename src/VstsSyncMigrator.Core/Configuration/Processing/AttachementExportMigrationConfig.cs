using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class AttachementExportMigrationConfig : ITfsProcessingConfig
    {
        public string QueryBit { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public Type Processor
        {
            get { return typeof(AttachementExportMigrationContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}