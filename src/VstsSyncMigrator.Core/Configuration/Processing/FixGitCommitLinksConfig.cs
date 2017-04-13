using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class FixGitCommitLinksConfig : ITfsProcessingConfig
    {
        public string TargetRepository { get; set; }
        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(FixGitCommitLinks); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}