using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Processors;
using MigrationTools.Tools;

namespace VstsSyncMigrator.Core.Execution
{
    public abstract class TfsMigrationProcessorBase : MigrationProcessorBase
    {
        public TfsStaticTools TfsStaticEnrichers { get; private set; }

        protected TfsMigrationProcessorBase(IMigrationEngine engine, TfsStaticTools tfsStaticEnrichers,  StaticTools staticEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<MigrationProcessorBase> logger) : base(engine, staticEnrichers, services, telemetry, logger)
        {
            TfsStaticEnrichers = tfsStaticEnrichers;
        }
    }
}
