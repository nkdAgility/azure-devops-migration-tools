using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Processors;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;

namespace VstsSyncMigrator.Core.Execution
{
    public abstract class TfsMigrationProcessorBase : MigrationProcessorBase
    {
        public TfsStaticEnrichers TfsStaticEnrichers { get; private set; }

        protected TfsMigrationProcessorBase(IMigrationEngine engine, TfsStaticEnrichers tfsStaticEnrichers,  StaticEnrichers staticEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<MigrationProcessorBase> logger) : base(engine, staticEnrichers, services, telemetry, logger)
        {
            TfsStaticEnrichers = tfsStaticEnrichers;
        }
    }
}
