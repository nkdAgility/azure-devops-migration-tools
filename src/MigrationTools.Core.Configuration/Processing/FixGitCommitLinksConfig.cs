using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class FixGitCommitLinksConfig : ITfsProcessingConfig
    {
        public string TargetRepository { get; set; }
        public bool Enabled { get; set; }
        public string QueryBit { get; set; }
        /// <inheritdoc />
        public string OrderBit { get; set; }

        public string Processor
        {
            get { return "FixGitCommitLinks"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}