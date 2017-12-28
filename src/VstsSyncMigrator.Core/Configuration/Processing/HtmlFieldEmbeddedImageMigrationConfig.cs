using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class HtmlFieldEmbeddedImageMigrationConfig : ITfsProcessingConfig
    {
        public string Status
        {
            get { return "Experimental"; }
        }

        public bool Enabled { get; set; }
        public string QueryBit { get; set; }

        public Type Processor
        {
            get { return typeof(HtmlFieldEmbeddedImageMigrationContext); }
        }

        /// <summary>
        /// Username used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        public string AlternateCredentialsUsername { get; set; }

        /// <summary>
        /// Password used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        public string AlternateCredentialsPassword { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}
