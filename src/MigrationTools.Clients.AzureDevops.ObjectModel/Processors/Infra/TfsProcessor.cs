using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools.Enrichers;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class TfsProcessor : Processor
    {
        protected TfsProcessor(IOptions<ProcessorOptions> options, TfsStaticTools tfsStaticTools, StaticTools staticTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, staticTools, processorEnrichers, services, telemetry, logger)
        {
            TfsStaticTools = tfsStaticTools;
        }

        public TfsStaticTools TfsStaticTools { get; private set; }
 
    }
}
