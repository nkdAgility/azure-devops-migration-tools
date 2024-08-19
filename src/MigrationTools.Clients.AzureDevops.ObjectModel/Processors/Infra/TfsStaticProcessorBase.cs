using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;


namespace MigrationTools.Processors.Infrastructure
{
    public abstract class TfsStaticProcessorBase : StaticProcessorBase
    {
        protected TfsStaticProcessorBase(TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<TfsStaticProcessorBase> logger) : base(staticEnrichers, services, me, telemetry, logger)
        {
            TfsStaticEnrichers = tfsStaticEnrichers;
        }

        public TfsStaticTools TfsStaticEnrichers { get; private set; }



    }
}
