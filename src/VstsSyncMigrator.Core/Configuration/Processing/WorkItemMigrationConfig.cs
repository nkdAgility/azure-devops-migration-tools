using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemMigrationConfig : ITfsProcessingConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
        public bool UpdateSoureReflectedId { get; set; }
        public bool BuildFieldTable { get; set; }
        public bool AppendMigrationToolSignatureFooter { get; set; }

        public string QueryBit { get; set; }
        public bool Enabled { get; set; }

        public Type Processor => typeof(WorkItemMigrationContext);


        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            Trace.WriteLine($"Note: {GetType().Name} is not compatible with {typeof(WorkItemRevisionReplayMigrationConfig).Name}");
            return !otherProcessors.Any(x => x is WorkItemRevisionReplayMigrationConfig);
        }
    }
}