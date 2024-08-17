using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.Processors
{
    public class FixGitCommitLinksProcessorOptions : IProcessorConfig
    {
        public string TargetRepository { get; set; }
        public bool Enabled { get; set; }
        public string Query { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        public string Processor
        {
            get { return typeof(FixGitCommitLinksProcessor).Name; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}