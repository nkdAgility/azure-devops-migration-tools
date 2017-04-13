﻿using System;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemMigrationConfig : ITfsProcessingConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
        public bool UpdateSoureReflectedId { get; set; }

        public string QueryBit { get; set; }
        public bool Enabled { get; set; }

        public Type Processor => typeof(WorkItemMigrationContext);
    }
}