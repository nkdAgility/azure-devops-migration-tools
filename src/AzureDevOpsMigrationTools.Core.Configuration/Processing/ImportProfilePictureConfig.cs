using System;
using System.Collections.Generic;

namespace AzureDevOpsMigrationTools.Core.Configuration.Processing
{
    public class ImportProfilePictureConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "ImportProfilePictureContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}