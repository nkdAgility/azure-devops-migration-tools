using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TFS Test Variables Migration Processor that migrates test variables and their values between TFS/Azure DevOps projects.
    /// </summary>
    public class TfsTestVariablesMigrationProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Gets the processor identifier for test variables migration.
        /// </summary>
        /// <inheritdoc />
        public string Processor
        {
            get { return "TestVariablesMigrationContext"; }
        }
    }
}
