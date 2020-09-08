using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class NodeStructuresMigrationConfig : ITfsProcessingConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }
        public string[] BasePaths { get; set; }
        /// <inheritdoc />
        public string Processor
        {
            get { return "NodeStructuresMigrationContext"; }
        }                         
        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}