using System.Collections.Generic;
using MigrationTools.Configuration;

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

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}