using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class ExportProfilePictureFromADConfig : ITfsProcessingConfig
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PictureEmpIDFormat { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public Type Processor
        {
            get { return typeof(ExportProfilePictureFromADContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}