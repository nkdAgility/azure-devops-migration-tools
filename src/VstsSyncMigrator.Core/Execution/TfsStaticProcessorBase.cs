using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using VstsSyncMigrator._EngineV1.Processors;

namespace VstsSyncMigrator.Core.Execution
{
    public abstract class TfsStaticProcessorBase : StaticProcessorBase
    {
        protected TfsStaticProcessorBase(TfsStaticEnrichers tfsStaticEnrichers, StaticEnrichers staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<StaticProcessorBase> logger) : base(staticEnrichers, services, me, telemetry, logger)
        {
            TfsStaticEnrichers = tfsStaticEnrichers;
        }

        public TfsStaticEnrichers TfsStaticEnrichers { get; private set; }



    }
}
