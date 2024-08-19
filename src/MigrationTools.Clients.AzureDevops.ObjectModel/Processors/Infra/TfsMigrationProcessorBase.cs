using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools;

using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class TfsMigrationProcessorBase : MigrationProcessorBase
    {
        public TfsStaticTools TfsStaticTools { get; private set; }

        protected TfsMigrationProcessorBase(IMigrationEngine engine, TfsStaticTools tfsStaticTools,  StaticTools staticTools, IServiceProvider services, ITelemetryLogger telemetry, ILogger<MigrationProcessorBase> logger) : base(engine, staticTools, services, telemetry, logger)
        {
            TfsStaticTools = tfsStaticTools;
        }
    }
}
