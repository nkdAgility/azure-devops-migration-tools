using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class TfsTestVariablesMigrationProcessorOptions : ProcessorOptions
    {
        /// <inheritdoc />
        public string Processor
        {
            get { return "TestVariablesMigrationContext"; }
        }
    }
}
