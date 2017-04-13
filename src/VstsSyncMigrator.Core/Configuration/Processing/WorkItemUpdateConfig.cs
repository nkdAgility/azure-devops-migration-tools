using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateConfig : ITfsProcessingConfig
    {
        public bool WhatIf { get; set; }

        public string QueryBit { get; set; }

        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(WorkItemUpdate); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}