using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;

namespace MigrationTools.Engine.Containers.Tests
{
    public class SimpleProcessorConfigMock : IProcessorConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "SimpleProcessorMock"; }
        }

        public List<IProcessorEnricher> Enrichers { get ; set ; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}