using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class HtmlFieldEmbeddedImageMigrationConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }
        public string QueryBit { get; set; }

        public bool FromAnyCollection { get; set; }

        /// <inheritdoc />
        public Type Processor => typeof(HtmlFieldEmbeddedImageMigrationContext);

        /// <summary>
        /// Username used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        public string AlternateCredentialsUsername { get; set; }

        /// <summary>
        /// Password used for VSTS basic authentication using alternate credentials. Leave empty for default credentials 
        /// </summary>
        public string AlternateCredentialsPassword { get; set; }

        /// <summary>
        /// Use default credential for downloading embedded images in source project 
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Ignore 404 errors and continue on images that don't exist anymore
        /// </summary>
        public bool Ignore404Errors { get; set; }

        /// <summary>
        /// Delete temporary files that were downloaded
        /// </summary>
        public bool DeleteTemporaryImageFiles { get; set; }

        /// <summary>
        /// TFS Aliases to use to match source images (e.g. https://myserver.company.com)
        /// </summary>
        public string[] SourceServerAliases { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}
