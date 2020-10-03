using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class TestVariablesMigrationConfig : IProcessorConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "TestVeriablesMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}