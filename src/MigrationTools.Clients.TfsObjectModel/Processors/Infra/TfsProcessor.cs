using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Enrichers;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class TfsProcessor : Processor
    {
        protected TfsProcessor(IOptions<ProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
  
        }

        new public TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new public TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        new public TfsCommonTools CommonTools => (TfsCommonTools)base.CommonTools;

    }
}
