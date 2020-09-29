using MigrationTools.Core.Configuration;
using System.Collections.Generic;

namespace MigrationTools.Core.Engine.Containers.Tests
{
    public class SimpleConfigMock : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "SimpleProcessorMock"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}