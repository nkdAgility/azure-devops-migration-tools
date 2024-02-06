using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class FixGitCommitLinksConfig : IProcessorConfig
    {
        public string TargetRepository { get; set; }
        public bool Enabled { get; set; }
        public string Query { get; set; }

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