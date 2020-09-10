using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class TestVariablesMigrationConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "TestVeriablesMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}