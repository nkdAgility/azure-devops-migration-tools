using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemRevisionReplayMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
        public bool UpdateSoureReflectedId { get; set; }
        public Type Processor
        {
            get
            {
                return typeof(WorkItemRevisionReplayMigrationContext);
            }
        }

        public string QueryBit { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            Trace.WriteLine($"Note: {GetType().Name} is not compatible with {typeof(WorkItemMigrationConfig).Name}");
            return !otherProcessors.Any(x => x is WorkItemMigrationConfig);
        }
    }
}