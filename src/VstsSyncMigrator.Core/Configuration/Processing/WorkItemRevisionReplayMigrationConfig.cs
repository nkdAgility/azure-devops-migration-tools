using System;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemRevisionReplayMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateSoureReflectedId { get; set; }
        public Type Processor
        {
            get
            {
                return typeof(WorkItemRevisionReplayMigrationContext);
            }
        }

        public string QueryBit { get; set; }
    }
}