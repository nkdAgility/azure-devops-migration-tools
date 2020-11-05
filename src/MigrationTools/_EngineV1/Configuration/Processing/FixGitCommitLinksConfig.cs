using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class FixGitCommitLinksConfig : IProcessorConfig
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
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}